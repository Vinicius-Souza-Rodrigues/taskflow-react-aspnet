# SDD — TaskFlow

**Data:** 2026-09-09
**Metodologia:** SDD puro
**Baseado no ADR:** v1.0

---

## Estrutura de Arquivos Esperada

```
taskflow-react-aspnet/
├── backend/
│   ├── src/
│   │   ├── TaskFlow.Api/            # Controllers, Contracts/Requests, Contracts/Responses, Mapping, Program.cs
│   │   ├── TaskFlow.Domain/         # TaskItem, ValueObjects (TaskTitle/TaskDescription), Notification, ITaskRepository
│   │   └── TaskFlow.Infra/          # DbContext, TaskRepository (implementa ITaskRepository), Migrations
│   ├── tests/
│   │   └── TaskFlow.Api.Tests/      # Smoke tests (xUnit + WebApplicationFactory)
│   ├── docker/                      # Dockerfiles individuais (desde a Fase 0)
│   ├── TaskFlow.slnx
│   └── dotnet-tools.json            # manifest local do dotnet-ef
├── nginx/                       # config do Nginx (a partir da Fase 2, containerizado)
├── docker-compose.yml           # a partir da Fase 3
├── .github/workflows/           # pipelines de CI/CD (a partir da Fase 4)
├── frontend/                    # app React (a partir da Fase 5)
├── PRD.md, CONTEXT.md, ADR.md, ARCHITECTURE.md, SDD.md, ROADMAP.md, DECISIONS.md, ARCH-REVIEWS.md
└── CLAUDE.md, README.md
```

---

## Módulos

### TaskFlow.Domain

**Responsabilidade:** definir a entidade Task (classe `TaskItem`, para não colidir com
`System.Threading.Tasks.Task`), os value objects que protegem os campos primitivos
(`TaskTitle`, `TaskDescription`), o enum de status (`TaskItemStatus`), o mecanismo de
notificação de erros (`Notification`) e o contrato de persistência (`ITaskRepository`) —
sem depender de banco, HTTP ou qualquer infraestrutura concreta.

**Entrada:**
```
tipo: chamadas de fábrica dos value objects (ex.: TaskTitle.Create(value, notification))
formato: valores primitivos (string) + um Notification para coletar erros
restrições: title não vazio e <= 200 caracteres; description <= 2000 caracteres
```

**Saída:**
```
tipo: value object válido (TaskTitle/TaskDescription), ou null com erro(s) no Notification
formato: TaskItem { Id, Title: TaskTitle, Description: TaskDescription, Status, CreatedAt, UpdatedAt }
erros possíveis: acumulados em Notification.Errors — nenhuma exceção lançada para erro de validação esperado
```

**Regras que nunca podem ser violadas:**
- title nunca pode ser nulo ou vazio; nunca existe um `TaskItem` com título inválido
  (value objects garantem isso na fronteira — o Domain não reimplementa a checagem depois)
- status sempre um dos 3 valores do enum (TODO, IN_PROGRESS, DONE) — nunca string livre
- createdAt nunca é alterado depois de criado
- não existe transição de status proibida (qualquer valor -> qualquer valor é permitido)
- erro de validação esperado nunca vira exceção — sempre entra em um `Notification`

---

### TaskFlow.Infra

**Responsabilidade:** implementar `ITaskRepository` (contrato definido no Domain) e
persistir/recuperar `TaskItem` no PostgreSQL via EF Core, e nada mais (sem regra de
negócio aqui).

**Entrada:**
```
tipo: entidade TaskItem (do Domain) para operações de escrita; filtros simples de id para leitura
formato: objeto TaskItem / int id
restrições: id, quando fornecido, precisa existir para update/delete/get-by-id
```

**Saída:**
```
tipo: entidade(s) TaskItem, ou null/not-found
formato: TaskItem ou IReadOnlyList<TaskItem>
erros possíveis: registro não encontrado (tratado como null; a tradução para HTTP 404 é responsabilidade da Api)
```

**Regras que nunca podem ser violadas:**
- toda mudança de schema é uma migration do EF Core, versionada e commitada
- nenhuma query SQL manual/DDL direta em produção
- `TaskRepository` é a única classe que conhece `TaskFlowDbContext` — mais nada no Infra
  ou na Api acessa o `DbContext` diretamente

---

### TaskFlow.Api

**Responsabilidade:** expor os 5 endpoints REST, validar entrada HTTP delegando aos
value objects do Domain, orquestrar `ITaskRepository`, e mapear resultado/erro para
status HTTP. Depende só de `TaskFlow.Domain` (nunca de `TaskFlow.Infra` diretamente) —
a única exceção é `Program.cs`, que registra a implementação concreta (`TaskRepository`,
`TaskFlowDbContext`) no container de DI.

**Entrada:**
```
tipo: requisições HTTP (JSON)
formato: ver Contrato da API abaixo
restrições: Content-Type application/json
```

**Saída:**
```
tipo: respostas HTTP (JSON)
formato: ver Contrato da API abaixo
erros possíveis: 400 (validação), 404 (id não encontrado), 500 (erro não esperado)
```

**Regras que nunca podem ser violadas:**
- nunca aceita id enviado pelo cliente na criação (POST) — id é sempre gerado pelo banco
- nunca aceita createdAt/updatedAt enviados pelo cliente — sempre definidos pelo servidor
- toda resposta de erro de validação retorna 400 com mensagem do campo inválido, nunca 500

---

## Contrato da API

| Método | Rota | Entrada (body) | Sucesso | Erros |
|---|---|---|---|---|
| POST | /api/tasks | `{ title, description?, status? }` | 201 + Task criada | 400 `{ errors: string[] }` se title ausente/vazio, description > 2000 ou status inválido |
| GET | /api/tasks | — | 200 + lista de Tasks (pode ser vazia) | — |
| GET | /api/tasks/{id} | — | 200 + Task | 404 se id não existe |
| PUT | /api/tasks/{id} | `{ title, description?, status }` | 200 + Task atualizada | 400 `{ errors: string[] }` se inválido, 404 se id não existe |
| DELETE | /api/tasks/{id} | — | 204 | 404 se id não existe |

O corpo de erro de validação (400) sempre traz **todos** os problemas encontrados de uma
vez em `errors` (Notification pattern) — não só o primeiro. Ex.:
`{"errors":["Invalid status: X","Title is required."]}`.

---

## Contratos entre Módulos

| De | Para | O que passa | Formato |
|---|---|---|---|
| Api | Domain | dados brutos do request + um `Notification` para construir os value objects | `TaskTitle?`, `TaskDescription?`, `TaskItemStatus`, erros em `Notification` |
| Api | Domain (`ITaskRepository`) | `TaskItem` para escrita, ou id para busca/remoção | `TaskItem` / `int` |
| Infra | Domain | implementação concreta de `ITaskRepository` (`TaskRepository`) | `TaskItem` / `IReadOnlyList<TaskItem>` / `null` |
| Program.cs (composition root) | Infra | registro de `TaskRepository`/`TaskFlowDbContext` no DI | — |

---

## Fora do Escopo Técnico

- Autenticação/autorização
- Paginação, filtro ou busca na listagem
- Máquina de estados de transição de status
- Soft delete (DELETE remove o registro de fato)
- Versionamento de API (ex.: /api/v2)

---

## Convenções obrigatórias

- **Nomenclatura:** C# em PascalCase para classes/métodos, camelCase no JSON exposto pela
  API (padrão do `System.Text.Json`).
- **Tratamento de erro:** regras de negócio (título obrigatório, tamanho, etc.) são
  validadas uma única vez, nos value objects do Domain (`TaskTitle`, `TaskDescription`),
  que reportam falhas via `Notification` — nunca lançando exceção para um erro de
  validação esperado. A Api nunca reimplementa essa validação — apenas verifica
  `Notification.IsValid` e traduz para 400 `{ errors: [...] }` com todos os problemas de
  uma vez; id não encontrado vira 404; qualquer outro erro (não esperado) vira 500
  genérico com log.
- **Logging:** log estruturado embutido do ASP.NET Core (`ILogger`) nesta fase; ferramenta
  dedicada (ex. Grafana/Loki) só a partir da Fase 7.
- **Outros:** strings de conexão e segredos nunca commitados — usar
  `appsettings.Development.json` (gitignored) localmente e variáveis de
  ambiente/secrets a partir da Fase 4 (Compose) e Fase 5 (CI).

# ADR — TaskFlow

**Data:** 2026-09-09 (atualizado 2026-09-11)
**Versão:** 1.1
**Baseado no PRD:** v1.0

---

## Núcleo do Domínio

- [x] Dados (armazenamento, transformação, consulta) — CRUD simples de Task, sem
  fluxo/processo real nem regras de negócio elaboradas.

---

## Complexidade de Estado

- [x] Simples — uma única entidade, sem relacionamentos, sem máquina de estados, sem
  concorrência relevante.

---

## Ciclo de Vida Esperado

- [x] Produto de longo prazo (uso pessoal) — o projeto evolui por fases ao longo de
  meses; não é descartável, mas também não é um produto para terceiros.

---

## Consumidor

- [x] Solo (só eu)

---

## Decisão de Metodologia

**Escolha:** SDD puro

**Justificativa:** spec clara (5 endpoints, 1 entidade, regras de validação simples),
output previsível, consumidor solo. Não há linguagem de domínio rica que justifique DDD,
nem integração frágil ou comportamento crítico que exija TDD como eixo principal — o risco
real do projeto está na infraestrutura (Fases 2-7), não na lógica de negócio (Fase 0-1).

**Atualização (2026-09-11):** a pedido explícito do usuário, para fins de aprendizado, o
backend foi refeito usando as ferramentas **táticas** de DDD por completo (Aggregate
Root, Domain Events, camada de Application com CQRS via MediatR) — ver
`DECISIONS.md`. Isso não muda a avaliação acima: o domínio continua simples o bastante
para não justificar a parte **estratégica** do DDD (Bounded Context, Aggregates com
invariante cruzando entidades) — essa parte foi deliberadamente descartada, não
esquecida.

**Postura de teste (projeção para o Roadmap):** smoke tests que confirmam o fluxo
principal (CRUD completo respondendo com os status HTTP corretos), sem gate
vermelho-verde bloqueando merge na `main`. A partir da Fase 5 (CI/CD) esses smoke tests
passam a rodar automaticamente a cada push, mas continuam informativos, não bloqueantes.

---

## Stack Decidida

| Camada | Tecnologia | Motivo |
|---|---|---|
| Backend / linguagem | C# / ASP.NET Core Web API (.NET 10, confirmado na Fase 0) | Identidade já definida pelo nome do repositório e confirmada pelo usuário; ecossistema .NET maduro para praticar infra |
| Estilo de API | Controllers (`[ApiController]`, MVC) | Escolha explícita do usuário — mais tradicional/familiar que Minimal APIs |
| Acesso a dados | Entity Framework Core | Escolha explícita; migrations versionadas nativas substituem o Flyway da proposta original |
| Banco de dados | PostgreSQL | Mantido da proposta original; open-source, roda bem em container, sem custo de licença |
| Reverse proxy | Nginx | Objetivo explícito de aprendizado — Fase 2 do Roadmap |
| Containerização | Docker | Objetivo explícito de aprendizado; antecipado para a Fase 0 (API e Postgres já sobem em containers desde o esqueleto — ver DECISIONS.md) |
| Orquestração local | Docker Compose | Objetivo explícito de aprendizado — Fase 3 |
| CI/CD | GitHub Actions | Repositório já está no GitHub; sem custo/infra extra para hospedar CI |
| Testes automatizados | xUnit | Padrão atual do template `dotnet new` para projetos .NET novos |
| Mediator/CQRS | MediatR | Padrão de mercado para Commands/Queries + dispatch de Domain Events em .NET; evita reinventar um dispatcher próprio (ver DECISIONS.md) |
| Frontend (fase futura) | React | Escolha explícita do usuário (identidade do repositório) — entra na Fase 5, depois da API estável |
| Prototipagem de UI (frontend) | Claude Design (canvas de design) | Telas do React (Fase 5) são prototipadas visualmente antes de virar código — decisão do usuário, evita implementar UI às cegas |
| Observabilidade (fase futura) | Grafana (+ stack de métricas a decidir na Fase 6) | Mencionado pelo usuário como melhoria futura; stack exata (Prometheus etc.) fica em aberto até a Fase 6 |

---

## Decisões Descartadas

| Opção | Motivo da rejeição |
|---|---|
| Java + Spring Boot | Substituído por ASP.NET Core depois do esclarecimento sobre o nome do repositório; usuário confirmou querer praticar .NET |
| Minimal APIs (ASP.NET) | Usuário preferiu Controllers/MVC por familiaridade |
| Dapper | Exigiria escrever SQL e migrations manualmente sem ganho real neste CRUD simples; EF Core já entrega migrations versionadas sem custo extra |
| Flyway | Redundante com EF Core Migrations, que já é nativo do .NET e cobre a mesma necessidade sem ferramenta adicional |
| SQLite | Não ensina infraestrutura de banco em container/rede — objetivo central do laboratório |
| Kubernetes agora | Complexidade desproporcional para as Fases 1-5; adiado explicitamente para a Fase 7 ("eventualmente", conforme o próprio usuário definiu) |
| Qualquer autenticação (JWT, sessão, OAuth) | Fora de escopo explícito do MVP — laboratório solo, sem necessidade de multiusuário |
| GUID como chave primária | Sem necessidade de geração distribuída de IDs neste escopo; inteiro autoincremento é mais simples e suficiente |
| CI/CD já na Fase 0 (walking skeleton "clássico") | O template padrão de bootstrap sugere CI/CD e deploy já no esqueleto inicial; aqui isso foi deliberadamente adiado para a Fase 4, porque o objetivo pedagógico do projeto é aprender cada peça de infra separadamente, uma fase de cada vez (ver DECISIONS.md) |
| Manter Docker isolado numa fase própria (não Fase 0) | Descartado a pedido do usuário: praticidade do ambiente de dev (evitar instalar .NET SDK/PostgreSQL direto no Windows) superou o valor pedagógico de isolar Docker numa fase única — ver DECISIONS.md |
| Dispatcher de Domain Events feito à mão (sem lib) | Reinventar um mini-mediator só pra 3 eventos com 1 handler cada é esforço sem ganho — MediatR já é o padrão testado do mercado .NET pra isso |
| Bounded Contexts / Aggregates com invariante cruzando entidades (DDD estratégico) | Só existe uma entidade e um contexto (Task) — aplicar a parte estratégica do DDD aqui seria puro over-engineering sem nenhum problema real pra resolver |
| Domain Events implementando `MediatR.INotification` diretamente | Manteria o Domain dependente de um framework externo; em vez disso `IDomainEvent` é um marcador puro, e só a Application (via `DomainEventNotification<T>`) conhece o MediatR |

---

## Fitness Functions

| Fitness Function | Característica protegida | Como checar |
|---|---|---|
| Domínio (entidade Task, enum Status) não referencia tipos de EF Core/Infra | Separação Domain/Infra | Revisão manual dos `using` no namespace de domínio |
| Toda mudança de schema passa por migration do EF Core, nunca DDL manual em produção | Rastreabilidade do banco | Revisão do histórico de migrations a cada fase |
| Contrato dos 5 endpoints REST não muda entre as Fases 1 e 5 | Estabilidade da API enquanto a infra evolui por baixo | Comparar payloads de request/response entre fases |
| Nenhuma fase de infra (Nginx/Docker/Compose/CI) exige mudança no código da API para funcionar | Camadas de infra desacopladas da aplicação | Revisão manual no gate de cada fase |

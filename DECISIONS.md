# DECISIONS — TaskFlow

> Registro de decisões estruturais do projeto.
> Este não é um changelog de código — é um registro de intenção.
> Atualizado sempre que houver mudança que afete arquitetura, modelo de dados, ou fluxo
> principal.

---

## Como usar

**Quando registrar:**
- Mudança na estrutura de arquivos
- Mudança em contrato de módulo (entrada/saída)
- Adição ou remoção de dependência relevante
- Adaptação de regra do SDD
- Decisão de não implementar algo que estava planejado

**Quando NÃO registrar:**
- Bugfix simples
- Ajuste de UI sem impacto em lógica
- Refactor interno sem mudança de contrato

---

## Entradas

### 2026-09-10 — Polish do frontend: cursor sistêmico, tokens sem cor crua, tema claro/escuro

**O que mudou:**
- Regra sistêmica em `index.css` (`@layer base`) para `cursor: pointer` em todo
  `button`/`select`/`[role="button"]` habilitado, e `cursor: not-allowed` quando
  desabilitado — em vez de espalhar `cursor-pointer` classe por classe (garante que
  nenhum componente futuro esqueça isso).
- Removida uma cor crua fora do sistema de tokens (`hover:bg-red-50` no botão de
  excluir) que não teria variante escura — trocada por `bg-danger/10`.
- Removidos utilitários `focus-visible:outline-*` redundantes em inputs/select — a regra
  global em `@layer base` já cobre foco em qualquer elemento; mantida só onde o
  comportamento é diferente do padrão (ex.: revelar o botão de excluir ao focar).
- Borda do campo de título fica `border-danger` quando há erro de validação (antes só a
  mensagem abaixo mudava) — alinhado ao que o próprio `DESIGN.md` já pedia.
- Chevron (`lucide-react`) sobreposto ao `<select>` nativo de status, com
  `appearance-none` — mantém o `<select>` 100% nativo/acessível, só troca a seta padrão
  do navegador por uma consistente com o resto do ícone-set.
- Hover sutil no `TaskCard` (borda `primary/30` + sombra leve) — antes só o botão de
  excluir reagia a hover, o card inteiro não dava nenhum feedback.
- **Tema claro/escuro** com alternância manual (`ThemeToggle` no header,
  Sol/Lua), persistida em `localStorage`, com fallback para
  `prefers-color-scheme` no primeiro acesso e script inline no `index.html` para não
  piscar o tema errado no carregamento. Implementado só redefinindo as variáveis CSS sob
  `:root.dark` — nenhum componente precisou mudar, porque todos já usavam tokens
  semânticos (`bg-surface`, `text-ink`) em vez de cor literal.

**Por que:** pedido do usuário para capricho visual + garantir integridade do CSS (uma
cor fora do sistema de tokens quebraria silenciosamente no tema escuro). O tema
claro/escuro só foi possível sem retrabalho justamente por já existir um sistema de
tokens central desde o início.

**Alternativa descartada:** `cursor-pointer` manual em cada componente (mais fácil
esquecer em componentes novos); dark mode via `prefers-color-scheme` apenas, sem toggle
manual (usuário pediu explicitamente a opção de alternar).

**Impacto:** `DESIGN.md` atualizado com a paleta escura completa e o mecanismo de
alternância. Nenhuma mudança de contrato com o backend.

**Como reverter:** remover `:root.dark {}` de `index.css`, o `ThemeToggle` do `App.tsx`
e o script inline do `index.html`.

---

### 2026-09-10 — Stack e decisões do frontend (Fase 5); bug de `Description` nulo corrigido

**O que mudou:** implementação completa do frontend (quadro Kanban de Tasks), com stack
e decisões técnicas registradas:

- **Stack:** Vite + React 19 + TypeScript, Tailwind CSS v4 (`@theme` em
  `src/index.css`, sem `tailwind.config.js` — v4 não usa mais esse arquivo para tokens),
  TanStack Query (estado de servidor: fetch/mutations/invalidação, evita
  useState+useEffect manual), `lucide-react` (ícones), `@fontsource/inter`
  (fonte self-hosted, sem CDN externo).
- **Padrão de UI:** quadro Kanban de 3 colunas (TODO/IN_PROGRESS/DONE) em vez de lista
  filtrável — API não suporta filtro por status, então agrupar por status no cliente
  depois de buscar tudo é natural, e o padrão Kanban é o mais popular/reconhecível para
  exatamente 3 estados fixos (Trello, Linear, GitHub Projects).
- **Criação de task é inline, nunca modal** — decisão do guia de UI de produto
  consultado (Impeccable): "modal é preguiça, esgotar alternativas inline antes".
  Formulário expande dentro da própria coluna TODO.
- **Mudança de status via `<select>` nativo** em vez de drag-and-drop — mais acessível
  via teclado, sem dependência de lib de D&D, visualmente ainda um badge colorido por
  cima do elemento nativo.
- **Estrutura de pastas:** `types/`, `lib/`, `api/`, `hooks/`, `components/` — divisão
  por tipo de responsabilidade, proporcional a um app de uma tela só.

**Bug encontrado e corrigido durante o teste manual:** `GET /api/tasks` retornava 500
quando alguma task tinha `description = null`. Causa: o EF Core, ao materializar uma
coluna `NULL`, atribui `null` direto na propriedade e **pula** o conversor customizado
(`HasConversion`) — como `TaskItem.Description` estava declarado como não-nulo
(`TaskDescription`, sem `?`), o `TaskResponseMapper` quebrava com
`NullReferenceException` ao acessar `.Value`. Corrigido declarando a propriedade como
`TaskDescription?` e usando `task.Description?.Value` no mapper — reflete a realidade de
como o EF Core já se comportava, em vez de brigar com esse comportamento.

**Bloqueio de ambiente:** o download do Chromium do Playwright
(`storage.googleapis.com`) falhou por timeout de rede duas vezes seguidas — bloqueio do
ambiente, não instabilidade passageira. Contornado usando o Microsoft Edge já instalado
na máquina via `channel: "msedge"` no `playwright.config.ts`, em vez do Chromium
bundlado. Os testes E2E rodam normalmente contra esse Edge.

**Alternativa descartada:** modal para criar task; drag-and-drop para mudar status;
Redux/Zustand para estado de servidor (TanStack Query resolve isso com menos código).

**Impacto:** `DESIGN.md` criado (fonte de intenção da paleta/tipografia/componentes).
`frontend/` deixa de ser placeholder. CORS habilitado na Api (`Program.cs`) para aceitar
`http://localhost:5173`.

**Como reverter:** não aplicável — é a primeira implementação do frontend.

---

### 2026-09-09 — Refactor SOLID/Object Calisthenics: Notification pattern, Repository, DTOs por arquivo

**O que mudou:** a pedido do usuário, revisão de clean code no backend:
- `DomainValidationException` foi removida. `TaskTitle` e `TaskDescription` viraram value
  objects que se autovalidam e reportam falhas via `Notification` (lista de erros), em vez
  de lançar exceção — a Api agora recebe todos os erros de validação de uma vez
  (`{"errors": [...]}`) em vez de só o primeiro.
- `ITaskRepository` foi criado no Domain; `TaskRepository` (Infra) o implementa. O
  `TasksController` passou a depender só da abstração (`ITaskRepository`), nunca mais do
  `TaskFlowDbContext` (tipo concreto do Infra) — Dependency Inversion Principle. Só
  `Program.cs` (composition root) ainda referencia `TaskFlow.Infra`.
- Mapeamento (status string↔enum, entidade→DTO) saiu do Controller para
  `Api/Mapping/TaskStatusMapper.cs` e `Api/Mapping/TaskResponseMapper.cs` — Single
  Responsibility.
- `Api/Contracts/TaskDtos.cs` (um arquivo genérico com 3 tipos) virou
  `Contracts/Requests/CreateTaskRequest.cs`, `Contracts/Requests/UpdateTaskRequest.cs` e
  `Contracts/Responses/TaskResponse.cs` — pastas com propósito explícito, um tipo por
  arquivo.

**Por que:** correção de um bug real de arquitetura (regra de validação duplicada entre
Api e Domain, achada na revisão anterior) e aplicação de SOLID/Object Calisthenics
pedida pelo usuário.

**Regras de Object Calisthenics aplicadas por inteiro:** sem `else`; um nível de
indentação por método; primitivos "perigosos" embrulhados em value objects (`TaskTitle`,
`TaskDescription`); nomes sem abreviação (`_taskRepository`, não `_repo`); `if` de uma
linha sem chaves.

**Regras aplicadas com critério, não ao pé da letra (e por quê):**
- *"No máximo 2 variáveis de instância por classe"* — `TaskItem` tem 6 (Id, Title,
  Description, Status, CreatedAt, UpdatedAt). Forçar isso ao pé da letra exigiria
  aninhar tudo em objetos artificiais (ex.: agrupar Id sozinho, título+descrição num
  "TaskContent", datas num "AuditInfo" aninhado) só para satisfazer a contagem — sem
  ganho real de clareza, e complicando o mapeamento do EF Core sem necessidade para um
  CRUD de 5 endpoints.
- *"Nenhum getter/setter"* — DTOs (`record`) e o EF Core exigem propriedades legíveis
  para serializar/mapear. Trocar por métodos tipo `GetTitle()` não mudaria nada de
  fundo, só trocaria sintaxe por sintaxe.
- *"Primeira classe para coleções"* — `ListAllAsync` retorna `IReadOnlyList<TaskItem>`
  direto; não criei um wrapper `TaskItemCollection` porque não há nenhum comportamento
  extra de coleção a esconder (é só listar).
- `TaskFlowDbContext`/`DbContext` mantém "Db" no nome — é a convenção do próprio EF
  Core; brigar com o nome da classe-base do framework não deixa nada mais legível.

**Alternativa descartada:** aplicar as 9 regras de Object Calisthenics ao pé da letra em
toda classe, mesmo onde isso adicionaria camadas sem reduzir ambiguidade real.

**Impacto:** `ARCHITECTURE.md` (componentes/invariantes atualizados para refletir a
inversão de dependência), `SDD.md` (módulos, contrato de erro, contratos entre módulos),
migration nova (`AddValueObjectConversions`, vazia — só sincroniza o snapshot do EF Core
com a metadata dos conversores, schema do banco não mudou).

**Como reverter:** voltar para `DomainValidationException`/exceções e para o acesso
direto ao `TaskFlowDbContext` no Controller; não recomendado — perde a cobertura de
múltiplos erros de uma vez e reintroduz o acoplamento Api→Infra.

---

### 2026-09-09 — `depends_on` do Compose não espera o Postgres ficar pronto (healthcheck adicionado)

**O que mudou:** a primeira subida do `docker compose up -d` (Fase 3) falhou — o
container da API iniciou e tentou aplicar migrations antes do Postgres aceitar conexões,
derrubando a API com `Npgsql.NpgsqlException: Connection refused`. Corrigido adicionando
um `healthcheck` (`pg_isready`) ao serviço `postgres` e trocando o `depends_on` da API
para `condition: service_healthy`.

**Por que:** `depends_on` sem `condition` no Docker Compose só garante a ORDEM de início
dos containers, não que o serviço dependido já esteja pronto para receber conexões —
Postgres continua inicializando por alguns segundos depois do container "iniciar". Isso é
uma pegadinha clássica de Compose, e exatamente o tipo de aprendizado de infra que este
projeto existe para capturar.

**Alternativa descartada:** um `sleep`/retry manual no entrypoint da API (mais frágil e
menos idiomático que um healthcheck nativo do Compose).

**Impacto:** `docker-compose.yml` — serviço `postgres` ganhou `healthcheck`; serviço
`api` mudou `depends_on` para a forma longa com `condition: service_healthy`.

**Como reverter:** remover o `healthcheck` e voltar ao `depends_on` curto (não
recomendado — reintroduz a race condition).

---

### 2026-09-09 — Projeto separado em `backend/` e `frontend/`; UI prototipada no Claude Design

**O que mudou:** duas decisões de organização, tomadas juntas ao fechar a Fase 0: (1) o
código .NET inteiro (`src/`, `tests/`, `docker/`, `TaskFlow.slnx`, `dotnet-tools.json`)
foi movido para `backend/`, com um `frontend/` vazio criado como placeholder para a
Fase 5; os documentos de bootstrap (`*.md`) continuam na raiz do repositório. (2) o
frontend React será prototipado visualmente no Claude Design antes de qualquer código —
o backend (Fases 0-4) é concluído por inteiro antes de a Fase 5 (frontend) começar.

**Por que:** o nome do repositório (`taskflow-react-aspnet`) já sugeria um monorepo
backend/frontend; separar cedo evita reorganizar depois com mais código escrito. O
usuário prefere ver o design da UI pronto (via Claude Design) antes de gerar componentes
React, e prefere fechar toda a infraestrutura de backend antes de abrir a frente de
frontend, mantendo o foco sequencial que já era a proposta original do projeto.

**Alternativa descartada:** manter tudo num único nível de pastas (`src/`, `tests/` na
raiz) até a Fase 5 chegar; implementar o React "no escuro", sem protótipo visual prévio.

**Impacto:** `SDD.md` (estrutura de arquivos) e `README.md` (todos os comandos
docker/dotnet) atualizados para os novos caminhos com prefixo `backend/`. `ROADMAP.md`
Fase 5 ganhou uma tarefa nova (protótipo no Claude Design) antes do scaffold do React.
`ADR.md` — Stack Decidida ganhou a linha "Prototipagem de UI".

**Como reverter:** mover o conteúdo de `backend/` de volta para a raiz; remover a tarefa
de prototipagem da Fase 5 do Roadmap.

---

### 2026-09-09 — Docker antecipado para a Fase 0 (substitui a decisão abaixo)

**O que mudou:** a decisão anterior ("Fase 0 não inclui CI/CD nem Docker", registrada mais
abaixo neste mesmo dia) adiava Docker para o que era a Fase 3. O usuário pediu
explicitamente que tanto a API quanto o PostgreSQL já rodem em containers Docker
separados desde a Fase 0, para não precisar instalar o .NET SDK nem o PostgreSQL
diretamente no Windows. O Roadmap foi renumerado: a antiga "Fase 3 — Dockerizar cada
serviço" deixou de existir como fase própria (o trabalho foi absorvido pela Fase 0);
Compose, CI/CD, Frontend e Melhorias recuaram uma posição (agora Fases 3, 4, 5 e 6— total
7 fases, 0 a 6, em vez de 8).

**Por que:** praticidade do ambiente de desenvolvimento (não instalar SDK/banco no SO)
pesou mais, para o usuário, do que manter Docker isolado como uma fase de aprendizado
separada. O aprendizado de Docker passa a acontecer já na Fase 0; a Fase 2 (Nginx)
também nasce containerizada, já que a base já está em containers.

**Alternativa descartada:** manter o plano original (API e Postgres nativos no Windows
até a antiga Fase 3).

**Impacto:** `ROADMAP.md` renumerado. `SDD.md` (estrutura de pastas) e
`ARCHITECTURE.md` atualizados para refletir Docker desde a Fase 0. `ADR.md` atualizado
(Stack Decidida e Decisões Descartadas).

**Como reverter:** voltar ao setup nativo (instalar .NET SDK e Postgres no Windows) e
mover as tarefas de Dockerfile de volta para uma fase própria antes do Compose.

---

### 2026-09-09 — Fase 0 não inclui CI/CD nem Docker

> **Status:** parcialmente substituída pela decisão acima (2026-09-09 — Docker
> antecipado para a Fase 0). A parte sobre CI/CD continua válida — CI/CD segue fora da
> Fase 0. A parte sobre Docker foi revertida.

**O que mudou:** o modelo padrão de bootstrap sugere que a "Fase 0" (esqueleto rodando)
já suba com deploy e CI/CD configurados. Para o TaskFlow isso foi propositalmente
adiado: CI/CD só entra na Fase 5, Docker só na Fase 3.

**Por que:** o objetivo declarado do projeto é aprender cada peça de infraestrutura
(Nginx, Docker, Compose, CI/CD) isoladamente, uma de cada vez. Empacotar tudo já na
Fase 0 anteciparia o aprendizado e reduziria o valor pedagógico do roadmap faseado que o
próprio usuário desenhou.

**Alternativa descartada:** seguir o walking skeleton "clássico" com deploy + CI/CD
desde o dia 1.

**Impacto:** a Fase 0 só cobre API local + Postgres local + esqueleto de testes. Fases
posteriores assumem essa base sem infra extra.

**Como reverter:** mover as tarefas de Docker/CI/CD da Fase 3/5 para a Fase 0, se o
usuário decidir priorizar isso.

---

### 2026-09-09 — Stack trocada de Java/Spring Boot para C#/ASP.NET Core

**O que mudou:** a proposta original do usuário especificava Java + Spring Boot + Flyway
no backend. Depois de esclarecido o conflito com o nome do repositório
(`taskflow-react-aspnet`), a stack final decidida foi C#/ASP.NET Core + Entity Framework
Core, com frontend React entrando em fase posterior.

**Por que:** o nome do repositório já identificava a stack pretendida pelo usuário;
confirmado explicitamente na etapa de Stack Grill.

**Alternativa descartada:** Java + Spring Boot + Flyway (proposta inicial).

**Impacto:** todos os exemplos de código, nomes de projeto e convenções seguem C#/.NET a
partir daqui. Flyway foi substituído por EF Core Migrations (equivalente funcional
nativo do .NET).

**Como reverter:** não há código escrito ainda; reverter significaria refazer o ADR/SDD
com a stack Java antes de iniciar a Fase 0.

---

### 2026-09-09 — Frontend React adiado para Fase 6

**O que mudou:** apesar do nome do repositório sugerir um projeto full-stack desde o
início, o usuário confirmou que o frontend React só deve entrar depois que a API estiver
estável (depois da Fase 5 — CI/CD).

**Por que:** manter o foco do laboratório na infraestrutura de backend primeiro (Nginx,
Docker, Compose, CI/CD) antes de introduzir a complexidade de um frontend separado.

**Alternativa descartada:** desenvolver API e frontend em paralelo desde a Fase 1.

**Impacto:** o Roadmap tem uma Fase 6 dedicada só ao frontend, com seu próprio Stack
Grill quando chegar a hora (ferramentas de build/state management do React ainda não
decididas).

**Como reverter:** antecipar a Fase 6 para logo após a Fase 1, se o usuário quiser um
frontend mais cedo.

<!-- Novas entradas sempre no topo, mais recente primeiro -->

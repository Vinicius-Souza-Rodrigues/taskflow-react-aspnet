# ARCH-REVIEWS — TaskFlow

Log de Revisões de Arquitetura por fase (gate obrigatório ao final de cada fase do
[ROADMAP.md](ROADMAP.md), antes de abrir a próxima).

Novas entradas sempre no topo, mais recente primeiro.

## 2026-09-10 — Revisão pós Fase 5: Frontend React (Kanban)

**Nota de ordem:** a Fase 4 (CI/CD) segue com o gate bloqueado — falta a verificação real
no GitHub Actions, que depende de um push que o agente foi instruído a não fazer. A
Fase 5 avançou mesmo assim, a pedido explícito do usuário.

**1. Drift vs ADR:** alinhado. Stack usada (Vite, React 19, TypeScript, Tailwind v4,
TanStack Query) bate com a decisão registrada em `DECISIONS.md`. `DESIGN.md` criado como
planejado no ADR (linha "Prototipagem de UI").

**2. Complexidade/acoplamento:** estável. Frontend é um projeto isolado (`frontend/`),
zero acoplamento de build com o backend; conversa com a API só via `fetch` HTTP. Módulos
internos (`types/lib/api/hooks/components`) com responsabilidade única cada.

**3. Dependências novas:** `react`, `react-dom`, `@tanstack/react-query`, `lucide-react`,
`@fontsource/inter`, `tailwindcss`/`@tailwindcss/vite`, `@playwright/test` (dev) —
todas justificadas em `DECISIONS.md`.

**4. Fitness functions:**
| FF | Resultado | Evidência |
|----|-----------|-----------|
| Frontend não duplica regra de validação do backend | pass | única validação client-side é "título não vazio", mensagem local; o resto (tamanho, status) é sempre validado pela API e o erro repassado |
| Jornada crítica testada de ponta a ponta contra a API real | pass | Playwright: criar → mudar status → excluir, 2/2 specs passando |
| Responsivo nos 3 breakpoints do `DESIGN.md` | pass | screenshots 375/768/1440 revisados manualmente |
| Build/lint limpos | pass | `tsc -b && vite build` e `oxlint` sem erros/warnings |

**Encaminhamentos:** bug de `Description` nulo (EF Core pulando o value converter em
coluna NULL) encontrado durante o teste manual e corrigido — ver `DECISIONS.md`. Download
do Chromium do Playwright bloqueado pela rede do ambiente — contornado usando o Edge do
sistema (`channel: "msedge"`), registrado como desvio aceito, não bloqueio.

**Veredito:** arquitetura saudável. Aprovação visual final ainda depende do usuário (ver
`DESIGN.md` § Visual Gate) antes de considerar a Fase 5 formalmente fechada.

---

## 2026-09-09 — Revisão pós Fase 3: Docker Compose

**1. Drift vs ADR:** alinhado, com um ajuste registrado em `DECISIONS.md`: a primeira
subida do `docker compose up -d` falhou (API tentou migrar antes do Postgres aceitar
conexões — `depends_on` sem condição só espera o container iniciar, não ficar pronto).
Corrigido com `healthcheck` no Postgres (`pg_isready`) e
`depends_on: condition: service_healthy` na API.

**2. Complexidade/acoplamento:** estável. `docker-compose.yml` só orquestra os 3
containers já existentes; nenhum módulo novo de aplicação.

**3. Dependências novas:** nenhuma. Segredos (usuário/senha/banco) movidos para
`.env`/`.env.example`, conforme já previsto no SDD para esta fase.

**4. Fitness functions:**
| FF | Resultado | Evidência |
|----|-----------|-----------|
| Contrato dos 5 endpoints estável | pass | 18/18 checks via Nginx/Compose |
| Migrations aplicadas automaticamente, nunca DDL manual | pass | `db.Database.Migrate()` no startup da API |
| Persistência entre reinícios | pass | task criada sobreviveu a `docker compose down` + `up` |
| Infra não exige mudança na API além da já prevista | pass | única mudança de código foi o auto-migrate, tarefa explícita desta fase |

**Encaminhamentos:** nenhum bloqueio.

**Veredito:** arquitetura saudável para avançar para a Fase 4.

---

## 2026-09-09 — Revisão pós Fase 2: Nginx como reverse proxy

**1. Drift vs ADR:** alinhado. `nginx/nginx.conf` faz `proxy_pass` pelo nome do serviço
Docker (`taskflow-api`), não IP fixo, como planejado.

**2. Complexidade/acoplamento:** estável. Nginx é um container independente, só
depende do nome `taskflow-api` estar resolvível na rede `taskflow-net`.

**3. Dependências novas:** imagem `nginx:alpine` — já prevista no Stack Grill.

**4. Fitness functions:**
| FF | Resultado | Evidência |
|----|-----------|-----------|
| Contrato dos 5 endpoints estável | pass | 18/18 checks repetidos via `localhost:80` (Nginx), idênticos aos da Fase 1 |
| Infra não exige mudança na API | pass | nenhuma mudança em `TaskFlow.Api` para esta fase |

**Encaminhamentos:** nenhum bloqueio.

**Veredito:** arquitetura saudável para avançar para a Fase 3.

---

## 2026-09-09 — Revisão pós Fase 1: CRUD completo de Tasks

**1. Drift vs ADR:** alinhado. `TasksController` (Controllers/MVC) sobre `TaskFlowDbContext`,
validação manual (title obrigatório ≤200 chars, status num enum de 3 valores), sem
autenticação, sem paginação — exatamente o Contrato da API do SDD.

**2. Complexidade/acoplamento:** estável. Controller depende de `TaskFlowDbContext`
(Infra) e dos tipos de `TaskFlow.Domain`; nenhuma dependência nova de camada.

**3. Dependências novas:** nenhuma (só alinhamento de versão do
`Microsoft.EntityFrameworkCore.Relational` para 10.0.12, eliminando warning de conflito
de versão — não é dependência nova, é fixação de versão).

**4. Fitness functions:**
| FF | Resultado | Evidência |
|----|-----------|-----------|
| Domínio não referencia EF Core/Infra | pass | Controller (Api) é quem depende de Infra, não o Domain |
| Migrations versionadas | pass | nenhuma migration nova necessária (schema já cobria os 5 endpoints) |
| Contrato dos 5 endpoints estável | pass | testado end-to-end contra o container real: 18/18 checks (POST/GET/PUT/DELETE + 400/404) |
| Infra não exige mudança na API | pass | nenhuma mudança de Docker/rede nesta fase |

**Encaminhamentos:** nenhum bloqueio.

**Veredito:** arquitetura saudável para avançar para a Fase 2.

---

## 2026-09-09 — Revisão pós Fase 0: Esqueleto rodando (containerizado)

**1. Drift vs ADR:** alinhado. Stack usada bate com a tabela Decidida (ASP.NET Core
Controllers, EF Core, PostgreSQL, Docker desde a Fase 0). Item antes assumido (".NET LTS
vigente") confirmado como .NET 10 — ADR atualizado.

**2. Complexidade/acoplamento:** estável. 3 módulos (`TaskFlow.Api`, `TaskFlow.Domain`,
`TaskFlow.Infra`) respeitando a tabela de componentes do `ARCHITECTURE.md` — Domain sem
dependências, Infra depende só de Domain, Api depende de Domain e Infra.

**3. Dependências novas:** `Npgsql.EntityFrameworkCore.PostgreSQL`,
`Microsoft.EntityFrameworkCore.Design`, `Microsoft.AspNetCore.Mvc.Testing` — todas já
previstas e justificadas na tabela Stack Decidida do ADR. Nenhuma dependência fora do
combinado.

**4. Fitness functions:**
| FF | Resultado | Evidência |
|----|-----------|-----------|
| Domínio não referencia EF Core/Infra | pass | `TaskItem`/`TaskItemStatus` não têm `using` de EF Core |
| Migrations versionadas, nunca DDL manual | pass | `InitialCreate` gerada via `dotnet ef migrations add`, aplicada via `database update` |
| Contrato dos 5 endpoints estável Fases 1-5 | pendente | endpoints de CRUD ainda não existem (só `/health`) — aplica a partir da Fase 1 |
| Infra não exige mudança na API para funcionar | pass | Dockerfile/rede não exigiram nada além do registro padrão do `DbContext` via configuração |

**Encaminhamentos:** nenhum bloqueio. Renomeação `Task` → `TaskItem` (para evitar colisão
com `System.Threading.Tasks.Task`) e separação `backend/`/`frontend/` registradas no
`DECISIONS.md`.

**Veredito:** arquitetura saudável para avançar para a Fase 1.

<!-- Novas entradas sempre no topo, mais recente primeiro -->

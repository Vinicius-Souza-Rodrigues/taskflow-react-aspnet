# ROADMAP — TaskFlow

**Data:** 2026-09-09
**Total de fases:** 7 (Fase 0 a Fase 6)

> Renumerado em 2026-09-09: Docker foi antecipado para a Fase 0 (API e Postgres já
> containerizados desde o esqueleto). Ver `DECISIONS.md` — "Docker antecipado para a
> Fase 0".

---

## Como ler este roadmap

- **Critério de conclusão** = comportamento observável, não tarefa técnica
- **Dependência** = qual fase precisa estar concluída antes
- **Gate de arquitetura** = toda fase termina com a Revisão de Arquitetura registrada em
  `ARCH-REVIEWS.md` antes de abrir a próxima
- **[AFK]** = o agente completa e fecha sozinho — **[HITL]** = precisa de validação do
  usuário antes de avançar

---

## Fases

### Fase 0 — Esqueleto rodando (containerizado)

**Objetivo:** ter a API ASP.NET Core rodando dentro de um container Docker, conectada a
um container PostgreSQL separado (rede Docker manual), sem instalar .NET SDK nem
PostgreSQL diretamente no Windows.

**Critério de conclusão:**
> Eu consigo subir os dois containers (API + Postgres, na mesma rede Docker) e acessar
> `GET /health` publicado pela API containerizada, com a migration inicial do EF Core já
> aplicada no Postgres (tabela Tasks criada, vazia).

**Tarefas para o agente:**
1. [AFK] Criar a solução .NET com os 3 projetos do SDD (TaskFlow.Api, TaskFlow.Domain,
   TaskFlow.Infra) + projeto de testes, usando o SDK do .NET via container (`docker run`
   com a imagem `sdk`, sem instalar nada no host).
2. [AFK] Escrever o Dockerfile multi-stage da API (build + runtime).
3. [AFK] Subir um container PostgreSQL (imagem oficial) numa rede Docker dedicada, com
   volume nomeado para persistência.
4. [AFK] Configurar EF Core + Npgsql apontando para o container Postgres, gerar e aplicar
   a migration inicial.
5. [AFK] Adicionar endpoint de health-check.
6. [AFK] Configurar bind mount do código-fonte + `dotnet watch` no container da API, para
   permitir hot-reload durante o desenvolvimento sem rebuild de imagem a cada mudança.
7. [AFK] Documentar no README os comandos exatos (`docker build`/`docker run`/`docker
   network`) usados para subir os dois containers.
8. [HITL] Confirmar que a API containerizada responde e conecta no Postgres
   containerizado antes de avançar para a Fase 1.

**Dependência:** nenhuma (depende do Docker Desktop, já disponível nesta máquina)
**Gate de arquitetura:** [x] Revisão de Arquitetura feita e registrada em `ARCH-REVIEWS.md` (2026-09-09) — confirmado pelo usuário (`/health` → `200 {"status":"healthy"}`, tabela `Tasks` criada)

---

### Fase 1 — CRUD completo de Tasks

**Objetivo:** os 5 endpoints REST funcionando de ponta a ponta, rodando dentro do
container da API.

**Critério de conclusão:**
> Eu consigo criar, listar, buscar por id, atualizar e remover uma Task via
> curl/Postman contra a API containerizada, e os dados persistem no container/volume do
> Postgres entre reinícios.

**Tarefas para o agente:**
1. [AFK] Implementar os 5 endpoints no TaskFlow.Api conforme o Contrato da API do SDD.
2. [AFK] Implementar validação de entrada (title obrigatório, status válido) retornando
   400 nos casos inválidos.
3. [AFK] Escrever smoke tests (xUnit) cobrindo o fluxo feliz dos 5 endpoints e os
   principais casos de erro (400/404).
4. [HITL] Validar manualmente os 5 endpoints (o usuário testa via Postman/curl) antes de
   fechar a fase.

**Dependência:** Fase 0
**Gate de arquitetura:** [x] Revisão de Arquitetura feita e registrada em `ARCH-REVIEWS.md` (2026-09-09) — 18/18 checks passando contra o container real (5 endpoints + validação + 404s)

---

### Fase 2 — Nginx como reverse proxy (containerizado)

**Objetivo:** acessar a API através de um container Nginx (porta 80) em vez de bater
direto na porta publicada da API, aprendendo `proxy_pass`, `location` e logs de acesso.

**Critério de conclusão:**
> Eu consigo rodar os 3 containers (API + Postgres + Nginx) na mesma rede Docker, acessar
> `localhost:80/api/tasks` através do Nginx e receber a mesma resposta que a porta
> publicada da API diretamente, com o log de acesso do Nginx registrando a requisição.

**Tarefas para o agente:**
1. [AFK] Escrever `nginx.conf` com `proxy_pass` apontando para o container da API pelo
   nome do serviço na rede Docker (não IP fixo).
2. [AFK] Subir o container Nginx (imagem oficial + config custom) na mesma rede dos
   outros dois.
3. [AFK] Configurar logs de acesso e erro do Nginx em um caminho conhecido/montado.
4. [HITL] Usuário confirma que a rota via Nginx funciona igual à rota direta.

**Dependência:** Fase 1
**Gate de arquitetura:** [x] Revisão de Arquitetura feita e registrada em `ARCH-REVIEWS.md` (2026-09-09) — 18/18 checks passando via `localhost:80`, logs de acesso confirmados

---

### Fase 3 — Docker Compose

**Objetivo:** os 3 containers (API, Postgres, Nginx) sobem com um único comando, em vez
de `docker run`/`docker network` manual para cada um.

**Critério de conclusão:**
> `docker compose up -d` sobe os 3 serviços, a API aplica as migrations automaticamente
> ao subir, e os dados do Postgres persistem em um volume nomeado entre
> `docker compose down` e `up` novamente.

**Tarefas para o agente:**
1. [AFK] Escrever `docker-compose.yml` com os 3 serviços, rede interna e volume nomeado
   para o Postgres (substituindo os comandos manuais da Fase 0-2).
2. [AFK] Configurar a API para aplicar migrations automaticamente na subida (dentro do
   container).
3. [AFK] Mover segredos (connection string) para variáveis de ambiente/`.env`
   (gitignored).
4. [HITL] Usuário confirma persistência de dados entre `down`/`up`.

**Dependência:** Fase 2
**Gate de arquitetura:** [x] Revisão de Arquitetura feita e registrada em `ARCH-REVIEWS.md` (2026-09-09) — 18/18 checks + persistência confirmada entre `down`/`up`

---

### Fase 4 — CI/CD com GitHub Actions

**Objetivo:** todo push/PR roda testes automaticamente, e todo merge na `main` builda a
imagem Docker da API.

**Critério de conclusão:**
> Um push no GitHub aciona o workflow, os smoke tests do TaskFlow.Api.Tests rodam e
> aparecem no PR, e um merge na `main` builda a imagem Docker com sucesso (publicar em
> registry é opcional nesta fase).

**Tarefas para o agente:**
1. [x] Criar workflow do GitHub Actions (`.github/workflows/ci.yml`): build + testes
   (.NET, `actions/setup-dotnet` + Postgres como service container) em todo push/PR.
2. [x] Adicionar step de build da imagem Docker em merges na `main`; `.github/dependabot.yml`
   para manter as actions atualizadas.
3. [ ] **[BLOQUEADO]** Usuário confirma o workflow rodando verde no GitHub — exige
   commit+push, e o usuário pediu explicitamente para o agente não commitar (ver memória
   de feedback). Workflow escrito e comandos verificados localmente, mas nunca rodou de
   verdade no GitHub Actions.

**Dependência:** Fase 3
**Gate de arquitetura:** [ ] Bloqueado no item 3 acima — Fase 5 avançou em paralelo a
pedido do usuário, sem esperar este gate fechar

---

### Fase 5 — Frontend React

**Objetivo:** uma tela simples em React para listar e criar Tasks, consumindo a API
existente.

**Critério de conclusão:**
> Eu consigo abrir a tela React, ver a lista de Tasks vindas da API, criar uma nova Task
> pela tela e vê-la aparecer na lista sem recarregar a página manualmente.

**Tarefas para o agente:**
1. ~~[HITL] Prototipar as telas no Claude Design antes de qualquer código~~ — decisão
   revertida pelo usuário em 2026-09-10 (ver `DECISIONS.md`): implementação direta em
   código, com `DESIGN.md` escrito antes como base de cores/tipografia/componentes em
   vez de protótipo visual separado.
2. [x] Escrever `DESIGN.md` (paleta OKLCH, tipografia, estados de componente, gate
   responsivo) antes do código.
3. [x] Scaffold do app React em `frontend/` — Vite + React 19 + TypeScript + Tailwind v4
   + TanStack Query + lucide-react (Stack Grill registrado em `DECISIONS.md`).
4. [x] Quadro Kanban (TODO/IN_PROGRESS/DONE) consumindo `GET /api/tasks`.
5. [x] Criação inline consumindo `POST /api/tasks`; mudança de status via `PUT`; exclusão
   via `DELETE` — CRUD completo na UI, não só criar/listar.
6. [x] Testes E2E com Playwright (jornada crítica: criar → mudar status → excluir) +
   revisão visual em 375/768/1440px.
7. [HITL] Usuário valida a tela funcionando contra a API real — dev server em
   `http://localhost:5173` (`npm run dev` dentro de `frontend/`).

**Dependência:** Fase 4 (API estável, já com CI/CD)
**Gate de arquitetura:** [x] Revisão de Arquitetura feita e registrada em `ARCH-REVIEWS.md`
(2026-09-10) — critérios técnicos/funcionais verificados; aprovação visual final ainda
pendente do usuário (ver `DESIGN.md` § Visual Gate)

---

### Fase 6 — Melhorias de infraestrutura

**Objetivo:** aproximar o laboratório de uma operação "de produção de verdade": HTTPS,
health checks robustos, logs estruturados, monitoramento, e avaliar Kubernetes/GCP.

**Critério de conclusão:**
> A stack roda com HTTPS via Nginx, health checks configurados no Compose/orquestrador,
> logs estruturados navegáveis, e existe pelo menos um dashboard no Grafana mostrando uma
> métrica real da API.

**Tarefas para o agente:**
1. [HITL] Decidir com o usuário (novo Stack Grill pontual) as ferramentas desta fase:
   certificado HTTPS (self-signed vs Let's Encrypt/mkcert), stack de métricas (Prometheus
   + Grafana), e se/quando avaliar Kubernetes/GCP.
2. [AFK] Implementar o que for decidido, uma peça de cada vez.

**Dependência:** Fase 5
**Gate de arquitetura:** [ ] Revisão de Arquitetura feita e registrada em `ARCH-REVIEWS.md`

---

## Marcos de validação

| Após a Fase | Pergunta de validação |
|---|---|
| 1 | A API resolve o CRUD descrito no PRD? |
| 3 | A stack inteira sobe sozinha com um comando, sem passos manuais esquecidos? |
| 5 | O frontend consegue operar o dia a dia da ferramenta sem eu precisar usar curl/Postman? |
| Final | Eu sei explicar e reproduzir cada camada de infra que montei, do zero? |

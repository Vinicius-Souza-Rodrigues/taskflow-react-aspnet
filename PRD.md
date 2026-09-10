# PRD — TaskFlow

**Data:** 2026-09-09
**Versão:** 1.0

---

## Problema

Não existe uma dor de produto real a resolver — o "problema" que o TaskFlow resolve é a
falta de um projeto próprio, ponta a ponta, pequeno o suficiente para não distrair, que
sirva de veículo para praticar infraestrutura de verdade: proxy reverso, containerização,
orquestração local e CI/CD. O CRUD de tarefas existe para dar a esse laboratório algo
concreto para rodar — não é o objetivo em si.

---

## Usuários

- [x] Só eu

---

## Menor uso com valor real (MVP)

Uma API REST (ASP.NET Core + PostgreSQL) rodando localmente, com os 5 endpoints de Task
(criar, listar, buscar por id, atualizar, remover) funcionando de ponta a ponta e
testável via cliente HTTP (curl/Postman/Insomnia). Sem interface gráfica nesta primeira
fatia.

---

## Fora do escopo (agora)

- Autenticação/autorização (API aberta, uso local e pessoal)
- Múltiplos usuários/times (multi-tenant) — decorre diretamente de não ter autenticação
- Paginação, filtros ou busca na listagem (`GET /api/tasks` retorna todas as tasks; volume
  de uso é pessoal e pequeno)
- Frontend (React fica para uma fase posterior, depois da API estar estável — ver
  ROADMAP.md Fase 5)
- Máquina de estados para transição de status (qualquer status pode ir para qualquer
  status via PUT)
- Deploy em nuvem/produção real (GCP é mencionado como possibilidade "eventual" na
  Fase 6, sem compromisso de custo ou prazo agora)

---

## Restrições conhecidas

- Máquina de desenvolvimento: Windows, PowerShell.
- Alvo de execução em produção/infra (fases 2+): Linux, containers Docker.
- Versionamento: Git/GitHub. CI/CD: GitHub Actions (evita depender de infra própria para
  rodar CI).
- Sem orçamento definido para infra paga — tudo roda local/self-hosted até a Fase 7, onde
  nuvem é uma possibilidade futura em aberto.

---

## Critério de sucesso

Eu consigo rodar `docker compose up -d` e acessar a API de Tasks através do Nginx (não
direto na porta do backend), com o banco persistindo dados em volume, e o GitHub Actions
rodando testes e build da imagem a cada push — e eu consigo explicar como cada peça
(Nginx, Docker, Compose, CI/CD) se encaixa, porque eu montei cada uma manualmente, uma
fase de cada vez.

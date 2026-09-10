# CLAUDE.md — TaskFlow

Índice operacional para quem (ou o quê) for executar código neste projeto. Detalhes
completos vivem nos documentos linkados — este arquivo não duplica conteúdo, aponta para
ele.

## Objetivo do projeto

API de gerenciamento de Tasks (CRUD simples) cujo propósito real é servir de laboratório
de infraestrutura pessoal — Nginx, Docker, Docker Compose, CI/CD e, eventualmente,
observabilidade/Kubernetes. Ver [PRD.md](PRD.md).

## Stack

C#/.NET (ASP.NET Core Web API, Controllers) + Entity Framework Core + PostgreSQL. Infra:
Docker desde a Fase 0 (API e Postgres containerizados, sem instalar SDK/banco no host),
Nginx (Fase 2), Docker Compose (Fase 3), GitHub Actions (Fase 4). Frontend React entra só
na Fase 5. Tabela completa e alternativas descartadas em [ADR.md](ADR.md).

## Metodologia e testes

SDD puro. Smoke tests em xUnit cobrindo o fluxo principal dos 5 endpoints — sem gate
vermelho-verde bloqueando `main`. Rodar com:

```
dotnet test
```

## Estrutura de pastas

Ver "Estrutura de Arquivos Esperada" em [SDD.md](SDD.md).

## Contratos entre módulos e regras invioláveis

Ver [SDD.md](SDD.md) (seções "Módulos" e "Fora do Escopo Técnico") e a seção "Fitness
Functions" do [ADR.md](ADR.md).

## Glossário de domínio

Ver [CONTEXT.md](CONTEXT.md) — termos como Task, Status e as regras de validação que
viram código.

## Estado atual / roadmap

Ver [ROADMAP.md](ROADMAP.md) para as fases e critérios de conclusão. Ver
[DECISIONS.md](DECISIONS.md) para o histórico de decisões estruturais. Toda mudança
estrutural atualiza [ARCHITECTURE.md](ARCHITECTURE.md) no mesmo commit.

## Antes de cada fase

Revisão de arquitetura obrigatória ao final de cada fase, registrada em
[ARCH-REVIEWS.md](ARCH-REVIEWS.md).

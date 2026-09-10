# CONTEXT — TaskFlow

Glossário de termos que viram módulo, tabela, campo ou contrato de API. Definido antes do
SDD para eliminar ambiguidade (Domain Grill — Etapa 1.5 do bootstrap).

---

## Task

Unidade central de dado do sistema. Representa um item de trabalho que o usuário quer
acompanhar. Campos:

- **id**: inteiro, gerado pelo banco (autoincremento). Não é enviado pelo cliente na criação.
- **title**: string obrigatória, não vazia, até 200 caracteres.
- **description**: string opcional (pode ser nula ou vazia), sem limite de negócio definido
  (limite técnico de coluna: 2000 caracteres).
- **status**: um dos três valores do enum Status (ver abaixo). Obrigatório na atualização;
  se ausente na criação, assume `TODO` por padrão.
- **createdAt**: timestamp UTC, definido pelo servidor no momento da criação. Nunca alterado
  depois.
- **updatedAt**: timestamp UTC, definido pelo servidor igual a `createdAt` na criação, e
  atualizado pelo servidor em todo PUT bem-sucedido. Nunca enviado/alterado pelo cliente
  diretamente.

## Status

Enum com três valores possíveis: `TODO`, `IN_PROGRESS`, `DONE`.

Transição: **livre** — qualquer valor pode ir para qualquer outro valor via PUT, sem
máquina de estados. Não existe ordem obrigatória nem transição proibida (decisão explícita
do usuário — ver DECISIONS.md).

Um valor de status fora desses três é erro de validação (HTTP 400) — não é aceito
silenciosamente nem normalizado para o valor mais próximo.

## Laboratório de infraestrutura

O termo que define o propósito do projeto: cada fase do Roadmap existe para o usuário
praticar uma peça de infraestrutura específica (Nginx, Docker, Docker Compose, CI/CD,
observabilidade), usando o CRUD de Tasks apenas como carga de trabalho de exemplo.
Consequência prática: decisões de escopo do produto (Task) devem ser as mais simples
possíveis, para não competir por atenção com o objetivo real do projeto, que é a
infraestrutura.

## API aberta (sem autenticação)

Significa que qualquer requisição HTTP que alcançar a API é atendida, sem validação de
identidade, token ou sessão. Consequência: a API não deve ser exposta na internet pública
sem adicionar autenticação — é uma decisão válida apenas no contexto de laboratório
pessoal/local.

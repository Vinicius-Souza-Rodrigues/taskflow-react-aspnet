# Design Contract — TaskFlow

**Atualizado em:** 2026-09-10
**Registro:** product (ferramenta de uso pessoal — o design serve a tarefa, não é o produto)

---

## Experience Intent

O usuário abre a tela, vê o estado de todas as tasks num quadro Kanban (TODO / IN
PROGRESS / DONE) e consegue criar, mover e remover uma task sem pensar na interface.
Familiaridade importa mais que personalidade: quem já usou Trello, Linear ou GitHub
Projects deve se sentir em casa imediatamente. A ferramenta deve desaparecer atrás da
tarefa.

## Register and Signature

- **Register:** product
- **Signature flow:** criar uma task e vê-la aparecer na coluna certa, sem reload —
  esse é o momento que prova que a UI funciona de ponta a ponta com a API real.

## Principles and References

- **Do:** um quadro Kanban de 3 colunas fixas (TODO, IN_PROGRESS, DONE); criação inline
  (sem modal) no topo de cada coluna; densidade moderada — cards compactos, mas
  legíveis; mesma linguagem visual em todo botão/ícone/estado.
- **Do:** referências de tom — Linear, Trello, GitHub Projects, Notion (tags suaves).
- **Don't:** sem modal para criar task (Impeccable product register: "modal é
  preguiça" — usar formulário inline/progressivo primeiro).
- **Don't:** sem gradientes decorativos, sem sombra larga + borda ao mesmo tempo, sem
  cantos com `border-radius` acima de 16px em cards, sem ícone/ilustração "fofa".
- **Don't:** sem cor de "urgência" (vermelho) em lugar nenhum — não existe conceito de
  task bloqueada/atrasada no domínio.

## Executable Token Source

- **Canonical path:** `frontend/src/index.css` (bloco `@theme` — Tailwind v4 não usa mais
  `tailwind.config.js` para tokens de cor) — os valores abaixo são a fonte de intenção;
  o bloco `@theme` é a fonte executável. Qualquer mudança de cor/token acontece nos dois
  lugares juntos.
- **Modelo:** tokens estruturados (`@theme` do Tailwind) consumidos diretamente pelas
  classes utilitárias (`bg-primary`, `text-ink`, etc.) — sem CSS-in-JS, sem tokens
  duplicados em outro arquivo.

### Paleta (OKLCH — seed hue 230°, cobalt/indigo)

Tema claro é o padrão de intenção; tema escuro redefine as mesmas variáveis sob
`:root.dark` (mesmo hue 230°, invertendo a escala de luminância). Alternância manual via
botão no header (`ThemeToggle`), com persistência em `localStorage` e fallback para
`prefers-color-scheme` no primeiro acesso — nunca decide sozinho sem deixar o usuário
trocar.

| Token | Claro | Escuro | Uso |
|---|---|---|---|
| `bg` | `oklch(1 0 0)` | `oklch(0.16 0.012 230)` | fundo da página |
| `surface` | `oklch(0.975 0.006 230)` | `oklch(0.21 0.014 230)` | fundo das colunas do board |
| `surface-2` | `oklch(0.96 0.008 230)` | `oklch(0.26 0.016 230)` | fundo dos cards |
| `border` | `oklch(0.90 0.01 230)` | `oklch(0.34 0.018 230)` | hairlines, divisórias, borda de input |
| `ink` | `oklch(0.20 0.02 230)` | `oklch(0.94 0.006 230)` | texto principal (≥7:1 contra `bg`) |
| `muted` | `oklch(0.48 0.02 230)` | `oklch(0.68 0.015 230)` | texto secundário (≥3.5:1 contra `bg`) |
| `primary` | `oklch(0.55 0.13 230)` | `oklch(0.62 0.14 230)` | botão primário, foco, link — sempre texto branco em cima |
| `primary-hover` | `oklch(0.48 0.14 230)` | `oklch(0.72 0.15 230)` | hover/active (no escuro, hover fica mais claro, não mais escuro) |
| `danger` | `oklch(0.50 0.18 25)` | `oklch(0.62 0.19 25)` | apenas o ícone de excluir, só no hover — nunca preenchido |

### Cores de status (badges suaves — fundo pastel + texto saturado da mesma família)

No escuro o padrão se inverte: fundo mais saturado/escuro + texto claro (mesma
convenção usada por labels do GitHub em dark mode), não o pastel do claro.

| Status | Fundo (claro) | Texto (claro) | Fundo (escuro) | Texto (escuro) |
|---|---|---|---|---|
| TODO | `oklch(0.95 0.01 250)` | `oklch(0.42 0.03 250)` | `oklch(0.30 0.02 250)` | `oklch(0.82 0.03 250)` |
| IN_PROGRESS | `oklch(0.95 0.05 75)` | `oklch(0.45 0.14 75)` | `oklch(0.32 0.08 75)` | `oklch(0.85 0.10 75)` |
| DONE | `oklch(0.94 0.06 150)` | `oklch(0.40 0.13 150)` | `oklch(0.30 0.09 150)` | `oklch(0.82 0.11 150)` |

### Tipografia

- **Família única:** Inter (`@fontsource/inter`, self-hosted — sem dependência de CDN
  externo). Produto não precisa de par display+body.
- **Escala fixa (rem), não fluida** — títulos não usam `clamp()`. Proporção entre
  degraus: ~1.125–1.2.
- Título da página: `text-xl font-semibold` · Título de coluna: `text-sm font-semibold
  uppercase tracking-wide` · Título do card: `text-sm font-medium` · Corpo/descrição:
  `text-sm text-muted` · Metadados (datas): `text-xs text-muted`.

## Critical Components and States

Todo componente interativo cobre: **default, hover, focus, active, disabled, loading**
(quando aplicável). Nenhum componente sobe com só o estado feliz.

- **TaskCard:** default / hover (leve elevação + borda) / focus-visible (anel de foco
  `primary`) / dragging (opacidade reduzida, sombra) / deletando (fade-out).
- **Botão primário (Adicionar task):** default / hover / focus-visible / active /
  disabled (título vazio) / loading (spinner substitui o label, nunca soma aos dois).
- **StatusSelect (mudar status do card):** default / hover / focus / aberto — `<select>`
  nativo (acessível via teclado) com chevron sobreposto, sem reinventar o dropdown.
- **ThemeToggle:** default / hover / focus-visible — ícone alterna Sol/Lua conforme o
  tema ativo.
- **Formulário inline de criação:** fechado (só um botão "+ Adicionar task") / aberto
  (input + textarea + ações) / erro (mensagem abaixo do campo, borda `danger` só no
  campo, nunca a tela toda) / enviando.
- **Empty state por coluna:** texto curto que ensina o que fazer ali (“Nenhuma task por
  aqui ainda” + o próprio botão de adicionar), nunca só “vazio”.
- **Toast/erro de rede:** mensagem discreta no rodapé da tela quando a API não responde
  — nunca uma tela em branco.

## Responsive and Accessible Behavior

- **Desktop (≥1024px):** 3 colunas lado a lado, cada uma ocupando 1/3 do board.
- **Tablet/mobile (<1024px):** scroll horizontal com `scroll-snap` — uma coluna quase
  em tela cheia por vez (padrão Trello mobile), sem esconder nenhuma coluna atrás de
  menu.
- **Teclado:** toda ação (criar, mudar status, excluir) alcançável só com Tab/Enter/Esc;
  `focus-visible` sempre visível (anel `primary`, nunca `outline: none` sem substituto).
- **Contraste:** `ink` sobre `bg` ≥7:1; `muted` sobre `bg` ≥3.5:1; texto de badge de
  status sempre a cor saturada da tabela acima (nunca texto cinza sobre o fundo
  colorido do badge).
- **Movimento:** transições de 150–250ms (hover, abrir formulário, remover card);
  `@media (prefers-reduced-motion: reduce)` troca qualquer transição por corte seco.

## Visual Gate

- **Rotas:** única tela (`/`) — sem roteamento.
- **Viewports:** 375px (mobile), 768px (tablet), 1440px (desktop).
- **Temas/navegadores:** claro e escuro, alternância manual (`ThemeToggle`) persistida em
  `localStorage`. Microsoft Edge (via Playwright, `channel: "msedge"`) como navegador de
  referência para os testes — o download do Chromium do Playwright está bloqueado pela
  rede deste ambiente (ver `DECISIONS.md`).
- **Evidência:** screenshot dos 3 viewports acima + teste E2E cobrindo o fluxo de
  assinatura (criar → aparece na coluna certa → mudar status → excluir).
- **Decisão humana:** usuário aprova visualmente antes de considerar a Fase 5 fechada.

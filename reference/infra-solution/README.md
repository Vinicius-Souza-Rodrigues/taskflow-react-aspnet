# Gabarito — Nginx / Docker / Compose

Cópia congelada dos arquivos que **já estão rodando de verdade** no projeto. Nada aqui é
executado — é só referência pra você reconstruir os arquivos reais
(`nginx/nginx.conf`, `backend/docker/api.Dockerfile`, `docker-compose.yml`, `.env`) com
as próprias mãos, e conferir/comparar quando travar.

Sugestão de ordem (é a mesma ordem das Fases 0-3 do [ROADMAP.md](../../ROADMAP.md)):

## 1. Container do Postgres sozinho

Sem olhar o gabarito ainda: sobe um container Postgres na porta 5432, com usuário/senha
próprios, e confirma com `docker exec ... psql` que consegue conectar.

*Dica se travar:* precisa de `POSTGRES_DB`/`POSTGRES_USER`/`POSTGRES_PASSWORD` como
variáveis de ambiente da imagem oficial `postgres:17-alpine`.

## 2. Dockerfile da API

Escreva um `Dockerfile` que builda a API .NET. Duas perguntas pra se fazer:
- Por que **duas** imagens (`FROM ... AS build` e depois outro `FROM ... AS final`) em
  vez de uma só?
- O que precisa ser copiado da etapa de build pra etapa final, e o que **não** precisa
  (ex.: o SDK inteiro)?

*Gabarito:* [`api.Dockerfile`](api.Dockerfile).

## 3. Nginx na frente

Escreva um `nginx.conf` que recebe requisição na porta 80 e repassa pra API. Perguntas
pra se fazer:
- Se a API está em outro container, como o Nginx "acha" ela? (dica: nome do container +
  mesma rede Docker, não IP fixo)
- O que acontece se você esquecer os `proxy_set_header`? Testa sem eles e olha o que
  chega no header `Host` do lado da API.

*Gabarito:* [`nginx.conf`](nginx.conf).

## 4. Docker Compose juntando tudo

Escreva um `docker-compose.yml` com os 3 serviços. Antes de olhar o gabarito, **tente
sem `healthcheck`** e veja o que acontece — é bem provável que a API quebre tentando
migrar o banco antes do Postgres estar pronto pra aceitar conexão. Esse erro é
proposital: é a pegadinha mais clássica de Compose, e está documentada em
[`DECISIONS.md`](../../DECISIONS.md) (entrada "`depends_on` do Compose não espera o
Postgres ficar pronto"). Depois de ver o erro na prática, adiciona o `healthcheck` +
`depends_on: condition: service_healthy` e resolve.

*Gabarito:* [`docker-compose.yml`](docker-compose.yml) + [`.env.example`](.env.example).

---

Quando terminar cada etapa, roda os testes/checks que já existem no projeto pra
confirmar que sua versão funciona igual à real (`GET /health`, os 5 endpoints, e
`docker compose down && up` pra provar que os dados persistem). Se sua versão divergir
do gabarito mas funcionar igual, tudo bem — o importante é entender o porquê de cada
linha, não copiar.

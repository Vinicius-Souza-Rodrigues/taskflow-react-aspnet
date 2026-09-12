# Trilha de DevOps — usando o TaskFlow como laboratório

Objetivo: virar DevOps. Ponto de partida: redes e hardware já estudados, Docker
revisado (comandos soltos, sem o quadro completo ainda). Cada módulo abaixo tem uma
parte de teoria (a aula) e uma parte prática (aplicar/observar no próprio TaskFlow, ou
no exercício em [`reference/infra-solution/`](infra-solution/)).

Isso não é um curso genérico — a ordem segue exatamente as fases que o TaskFlow já foi
desenhado pra ensinar (ver [`ROADMAP.md`](../ROADMAP.md)).

## Progresso

- [ ] **1. Docker de verdade** — imagens, camadas, containers, multi-stage build, redes
  Docker, volumes. *(módulo atual — aula abaixo)*
- [ ] **2. Docker Compose** — orquestração declarativa, healthcheck, dependência entre
  serviços, variáveis de ambiente/secrets.
- [ ] **3. Nginx / reverse proxy** — proxy_pass, headers, logs, TLS/HTTPS, (mais adiante)
  load balancing.
- [ ] **4. CI/CD** — pipelines, GitHub Actions, o que roda em PR vs. o que roda em merge,
  build de imagem, gates de qualidade.
- [ ] **5. Observabilidade** — logs estruturados, métricas, Prometheus/Grafana,
  alerting: como saber que algo quebrou antes do usuário reclamar.
- [ ] **6. Infraestrutura como código** — Terraform (ou equivalente): parar de clicar em
  console de nuvem e passar a descrever a infra em arquivo versionado.
- [ ] **7. Fundamentos de nuvem** — escolher um provedor (GCP é o cotado no projeto),
  entender compute/storage/networking gerenciado, IAM básico.
- [ ] **8. Kubernetes** — só depois de Docker/Compose estarem sólidos: pods, services,
  deployments, o que resolve que o Compose não resolve.
- [ ] **9. Segurança de infraestrutura** — gestão de segredos, least privilege, scan de
  imagem, superfície de ataque de um container exposto.

## Módulo 1 — Docker de verdade

### Por que existe

Antes do Docker, "funciona na minha máquina" era um problema real: a aplicação dependia
de versões exatas de bibliotecas, do SO, de configuração manual. Docker resolve isso
empacotando a aplicação **e tudo que ela precisa pra rodar** numa unidade portátil — a
imagem.

### Imagem vs. container

- **Imagem** = a receita congelada (arquivos, dependências, comando de start). Não roda
  sozinha.
- **Container** = a imagem em execução — um processo isolado (namespaces do Linux:
  processo, rede, filesystem) rodando no kernel do seu host. **Não é uma VM** — não tem
  kernel próprio, é isolamento em cima do kernel que já existe. É por isso que um
  container sobe em milissegundos e uma VM leva segundos/minutos.

### Camadas (layers)

Cada instrução do Dockerfile (`FROM`, `RUN`, `COPY`...) cria uma **camada** — um diff do
filesystem. Camadas são cacheadas: se você não mudou uma linha do Dockerfile nem os
arquivos que ela copia, o Docker reusa a camada em vez de refazer o trabalho. É por isso
que a ordem das instruções importa — coisas que mudam pouco (restaurar dependências) vêm
antes de coisas que mudam sempre (copiar o código-fonte).

### Multi-stage build (o `api.Dockerfile` do projeto usa isso)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build   # estágio 1: tem o SDK inteiro (pesado)
...
RUN dotnet publish ...

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final # estágio 2: só o runtime (leve)
COPY --from=build /app/publish .                   # pega só o RESULTADO do estágio 1
```

Sem isso, a imagem final carregaria o SDK inteiro (compilador, ferramentas de build) só
pra rodar um `.dll` já compilado — desperdício de espaço e superfície de ataque maior
(mais coisa instalada = mais coisa que pode ter vulnerabilidade). O estágio final não
tem nada do estágio de build exceto o que foi explicitamente copiado.

### Rede Docker

Containers na mesma rede Docker se enxergam **pelo nome do container**, não por IP fixo
— o Docker roda um DNS interno. É por isso que `nginx.conf` do projeto tem
`proxy_pass http://taskflow-api:8080` em vez de um IP: o nome `taskflow-api` só resolve
porque os dois containers estão na mesma rede.

### Volumes

O filesystem de um container é efêmero — se ele morre, os dados dentro dele morrem
junto. Um **volume** é uma área do disco do host gerenciada pelo Docker, montada dentro
do container, que sobrevive à vida do container. É por isso que o Postgres do projeto
usa um volume nomeado (`taskflow-pgdata`) — sem isso, cada `docker compose down` apagaria
o banco inteiro.

### Prática deste módulo

1. Sem Compose ainda: sobe um Postgres sozinho (`docker run`), entra dentro do container
   rodando (`docker exec -it <nome> sh`) e explora o filesystem — acha onde os dados
   ficam.
2. Mata o container (`docker rm -f`) sem ter usado volume — os dados sumiram. Repete
   usando `-v algum-nome:/var/lib/postgresql/data` — mata de novo, sobe de novo, os
   dados continuam lá.
3. Escreve o `api.Dockerfile` do zero (exercício 2 do
   [`reference/infra-solution/README.md`](infra-solution/README.md)), rodando
   `docker build` a cada tentativa e lendo o erro quando quebrar — é assim que se
   aprende Docker de verdade, não decorando sintaxe.
4. Depois de buildar, roda `docker history <sua-imagem>` e olha o tamanho de cada
   camada — compara o tamanho da imagem final com o que seria só o SDK sozinho
   (`docker images | grep sdk`).

Quando terminar a prática, me chama pra seguirmos pro Módulo 2 (Compose) — ou pra tirar
dúvida de qualquer coisa que não fechou aqui.

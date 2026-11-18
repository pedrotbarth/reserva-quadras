# Sistema de Reserva de Quadras Esportivas

**Integrantes**  
Pedro Henrique Teixeira Barth, Marcelo Vinícius Batista Candido e João Vitor Goulart Viana

**Disciplina**  
Arquitetura de Software — Engenharia de Software, Unisatc

## Propósito

O sistema permite cadastrar clientes e quadras esportivas, reservar horários e confirmar reservas por meio de pagamentos simulados. Quando o horário solicitado está ocupado, a API sugere alternativas disponíveis na mesma quadra.

## Usuários

- **Cliente** — solicita e consulta reservas e registra o pagamento simulado.
- **Administrador** — gerencia os cadastros de clientes e quadras e acompanha as reservas.

## Requisitos funcionais

1. Cadastrar, listar, consultar e editar clientes.
2. Cadastrar, listar, consultar e editar quadras, com modalidade, valor por hora, horário de abertura e fechamento e situação ativa ou inativa.
3. Criar, listar, consultar e cancelar reservas.
4. Validar o cliente e a quadra ao criar uma reserva, verificar o horário de funcionamento e calcular o valor total.
5. Impedir reservas ativas sobrepostas para a mesma quadra.
6. Em caso de conflito, retornar `409 Conflict` e sugerir até três horários disponíveis no mesmo dia e na mesma quadra, mantendo a duração solicitada.
7. Registrar pagamentos simulados com status `Aprovado` ou `Recusado` e impedir mais de um pagamento aprovado para a mesma reserva.
8. Confirmar a reserva após a aprovação do pagamento, alterando seu status de `AguardandoPagamento` para `Confirmada`.

## Arquitetura

A aplicação usa quatro microsserviços independentes, desenvolvidos a partir do template estudado em sala. Cada serviço possui sua própria API REST em ASP.NET Core com .NET 10 e seu próprio banco SQLite, gerenciado pelo Entity Framework Core. A comunicação entre os serviços ocorre via HTTP.

| Microsserviço | Responsabilidade | Código |
|---|---|---|
| Clientes | Gerencia o cadastro e a situação dos clientes. | [`src/Clientes`](src/Clientes) |
| Quadras | Gerencia as quadras, os horários de funcionamento e os valores por hora. | [`src/Quadras`](src/Quadras) |
| Reservas | Valida cliente e quadra, calcula o valor da reserva, controla conflitos e sugere horários disponíveis. | [`src/Reservas`](src/Reservas) |
| Pagamentos | Valida a reserva, registra o pagamento simulado e solicita a confirmação quando ele é aprovado. | [`src/Pagamentos`](src/Pagamentos) |

## Integrações entre os microsserviços

| Origem | Destino | Operação |
|---|---|---|
| Reservas | Clientes | `GET /api/clientes/{id}` para verificar se o cliente existe e está ativo. |
| Reservas | Quadras | `GET /api/quadras/{id}` para consultar a situação da quadra, seu horário de funcionamento e o valor por hora. |
| Pagamentos | Reservas | `GET /api/reservas/{id}` para conferir o status e o valor total antes de registrar o pagamento. |
| Pagamentos | Reservas | `PATCH /api/reservas/{id}/confirmar-pagamento` para informar o ID do pagamento aprovado e confirmar a reserva. |

## Observabilidade

Cada serviço oferece a rota `GET /health`, que verifica o acesso ao SQLite. Os logs HTTP registram a rota, o status e o tempo de resposta. O cabeçalho `X-Correlation-Id` permite acompanhar a mesma requisição nos logs dos serviços envolvidos.

## Serviços publicados no Render

| Serviço | Verificação de saúde |
|---|---|
| Clientes | [Clientes — `/health`](https://reserva-quadras-clientes.onrender.com/health) |
| Quadras | [Quadras — `/health`](https://reserva-quadras-quadras.onrender.com/health) |
| Reservas | [Reservas — `/health`](https://reserva-quadras-reservas.onrender.com/health) |
| Pagamentos | [Pagamentos — `/health`](https://reserva-quadras-pagamentos.onrender.com/health) |

Os serviços são publicados usando um Dockerfile por microsserviço. No Render, Reservas recebe as URLs de Clientes e Quadras pelas variáveis `Clientes__UrlBase` e `Quadras__UrlBase`. Pagamentos recebe a URL de Reservas pela variável `Reservas__UrlBase`.

## Execução local

Instale o SDK do .NET 10. Na raiz do repositório, abra um terminal por serviço e execute o comando correspondente. Cada serviço aplica as migrações do SQLite ao iniciar.

| Serviço | Comando |
|---|---|
| Clientes | `dotnet run --project src/Clientes/Clientes.csproj --no-launch-profile -- --urls http://localhost:5101` |
| Quadras | `dotnet run --project src/Quadras/Quadras.csproj --no-launch-profile -- --urls http://localhost:5102` |
| Reservas | `dotnet run --project src/Reservas/Reservas.csproj --no-launch-profile -- --urls http://localhost:5103` |
| Pagamentos | `dotnet run --project src/Pagamentos/Pagamentos.csproj --no-launch-profile -- --urls http://localhost:5104` |

## Teste do fluxo completo

1. Cadastre um cliente em `POST /api/clientes` e uma quadra em `POST /api/quadras`.
2. Crie uma reserva em `POST /api/reservas` usando os IDs retornados e um horário dentro do funcionamento da quadra.
3. Registre um pagamento em `POST /api/pagamentos` informando `reservaId`, o `valorTotal` da reserva no campo `valor` e `formaPagamento` como `Pix`.
4. Consulte a reserva em `GET /api/reservas/{id}` e verifique se o status passou para `Confirmada`.

O fluxo completo foi testado no Render. O pagamento retornou `Aprovado` e a reserva ficou com status `Confirmada`.

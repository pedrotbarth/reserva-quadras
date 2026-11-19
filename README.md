# Sistema de Reserva de Quadras Esportivas

**Integrantes**  
Pedro Henrique Teixeira Barth  
Marcelo Vinícius Batista Candido  
João Vitor Goulart Viana

**Disciplina**  
Arquitetura de Software — Engenharia de Software, Unisatc

## Propósito

O sistema permite cadastrar clientes e quadras esportivas, reservar horários e confirmar reservas por meio de pagamentos simulados. Quando o horário solicitado está ocupado, a API sugere alternativas disponíveis na mesma quadra.

## Usuários

- **Cliente** solicita e consulta reservas e registra o pagamento simulado.
- **Administrador** gerencia os cadastros de clientes e quadras e acompanha as reservas.

## Requisitos funcionais

1. Cadastrar, listar, consultar e editar clientes.
2. Cadastrar, listar, consultar e editar quadras, informando modalidade, valor por hora, horário de abertura e fechamento e situação ativa ou inativa.
3. Criar, listar, consultar e cancelar reservas.
4. Validar o cliente e a quadra ao criar uma reserva, verificar o horário de funcionamento e calcular o valor total.
5. Impedir reservas ativas sobrepostas para a mesma quadra.
6. Em caso de conflito, retornar `409 Conflict` e sugerir até três horários disponíveis no mesmo dia e na mesma quadra, mantendo a duração solicitada.
7. Registrar pagamentos simulados com status `Aprovado` ou `Recusado` e impedir mais de um pagamento aprovado para a mesma reserva.
8. Confirmar a reserva após a aprovação do pagamento, alterando seu status de `AguardandoPagamento` para `Confirmada`.

## Arquitetura

A aplicação utiliza quatro microsserviços independentes, desenvolvidos a partir do template estudado em sala.

Cada serviço possui uma API REST em ASP.NET Core com .NET 10 e um banco SQLite próprio, gerenciado pelo Entity Framework Core. A comunicação entre os serviços ocorre via HTTP.

| Microsserviço | Responsabilidade | Código |
|---|---|---|
| Clientes | Gerencia o cadastro e a situação dos clientes. | [src/Clientes](src/Clientes) |
| Quadras | Gerencia as quadras, os horários de funcionamento e os valores por hora. | [src/Quadras](src/Quadras) |
| Reservas | Valida cliente e quadra, calcula o valor, controla conflitos e sugere horários disponíveis. | [src/Reservas](src/Reservas) |
| Pagamentos | Valida a reserva, registra o pagamento simulado e solicita a confirmação quando ele é aprovado. | [src/Pagamentos](src/Pagamentos) |

## Tecnologias utilizadas

- .NET 10 e ASP.NET Core.
- Entity Framework Core.
- SQLite.
- APIs REST com JSON.
- HttpClient para comunicação entre serviços.
- Scalar para documentação e testes das APIs.
- Docker para empacotamento.
- Render para publicação.
- GitHub para versionamento e documentação.

## Integrações entre os microsserviços

| Origem | Destino | Operação |
|---|---|---|
| Reservas | Clientes | `GET /api/clientes/{id}` para verificar se o cliente existe e está ativo. |
| Reservas | Quadras | `GET /api/quadras/{id}` para consultar a situação, o funcionamento e o valor por hora da quadra. |
| Pagamentos | Reservas | `GET /api/reservas/{id}` para conferir o status e o valor total antes de registrar o pagamento. |
| Pagamentos | Reservas | `PATCH /api/reservas/{id}/confirmar-pagamento` para informar o ID do pagamento aprovado e confirmar a reserva. |

As consultas de Reservas a Clientes e Quadras atendem às duas buscas entre microsserviços exigidas no trabalho. A confirmação enviada por Pagamentos a Reservas atende à alteração de dados entre serviços.

## Observabilidade

Cada serviço oferece a rota `GET /health`, que verifica se consegue consultar seu banco SQLite.

Os logs HTTP registram o método, a rota, o status da resposta e a duração da chamada. O cabeçalho `X-Correlation-Id` permite acompanhar a mesma operação nos logs dos serviços envolvidos.

## Serviços publicados no Render

| Serviço | Scalar | Health check |
|---|---|---|
| Clientes | [Testar API](https://reserva-quadras-clientes.onrender.com/scalar) | [Verificar saúde](https://reserva-quadras-clientes.onrender.com/health) |
| Quadras | [Testar API](https://reserva-quadras-quadras.onrender.com/scalar) | [Verificar saúde](https://reserva-quadras-quadras.onrender.com/health) |
| Reservas | [Testar API](https://reserva-quadras-reservas.onrender.com/scalar) | [Verificar saúde](https://reserva-quadras-reservas.onrender.com/health) |
| Pagamentos | [Testar API](https://reserva-quadras-pagamentos.onrender.com/scalar) | [Verificar saúde](https://reserva-quadras-pagamentos.onrender.com/health) |

Cada microsserviço é publicado com seu próprio Dockerfile e aplica as migrações do SQLite durante a inicialização.

As URLs dos serviços são configuradas pelas variáveis de ambiente abaixo.

| Serviço | Variável | Valor configurado no Render |
|---|---|---|
| Reservas | `Clientes__UrlBase` | `https://reserva-quadras-clientes.onrender.com` |
| Reservas | `Quadras__UrlBase` | `https://reserva-quadras-quadras.onrender.com` |
| Pagamentos | `Reservas__UrlBase` | `https://reserva-quadras-reservas.onrender.com` |

## Como testar pelo Scalar

Abra os quatro links de health check e aguarde as respostas antes de começar.

No Scalar, selecione a operação, clique em **Test Request**, preencha o **Request Body** e clique em **Send**.

Confira o endereço da requisição dentro do Scalar. Ao testar no Render, ele deve começar com `https://`. Se aparecer `http://`, altere para `https://` antes de enviar.

Guarde os IDs retornados durante o teste, pois eles serão utilizados nas próximas etapas. Para repetir o roteiro, cadastre um novo cliente e uma nova quadra.

### 1. Cadastrar um cliente

Abra o Scalar de **Clientes** e execute `POST /api/clientes`.

```json
{
  "nome": "Cliente Demonstracao",
  "telefone": "48999999999",
  "email": "cliente.demonstracao@example.com"
}
```

O resultado esperado é `201 Created`, com os dados do cliente cadastrado.

Copie o campo `id` da resposta. Ele será utilizado como `clienteId` na reserva.

### 2. Cadastrar uma quadra

Abra o Scalar de **Quadras** e execute `POST /api/quadras`.

```json
{
  "nome": "Quadra Demonstracao",
  "modalidade": "Futsal",
  "valorPorHora": 120,
  "horarioAbertura": "08:00:00",
  "horarioFechamento": "22:00:00"
}
```

O resultado esperado é `201 Created`, com os dados da quadra cadastrada.

Copie o campo `id` da resposta. Ele será utilizado como `quadraId` na reserva.

### 3. Criar a reserva

Abra o Scalar de **Reservas** e execute `POST /api/reservas`.

Substitua os textos dos IDs pelos valores retornados nos passos anteriores. Ajuste a data para um dia futuro no formato `AAAA-MM-DD`.

```json
{
  "clienteId": "COLE_O_ID_DO_CLIENTE",
  "quadraId": "COLE_O_ID_DA_QUADRA",
  "data": "2026-11-26",
  "horarioInicio": "19:00:00",
  "horarioFim": "20:00:00"
}
```

O resultado esperado é `201 Created`, com os seguintes valores.

- `status` igual a `AguardandoPagamento`.
- `valorTotal` igual a `120`.

Copie o campo `id` da reserva para utilizar no pagamento.

Durante essa operação, Reservas realiza duas consultas entre microsserviços.

- Consulta Clientes para validar o cadastro.
- Consulta Quadras para validar a situação, conferir o funcionamento e obter o preço por hora.

### 4. Testar o conflito de horário

Envie novamente o mesmo pedido do passo 3, mantendo a quadra, a data e os horários.

O resultado esperado é `409 Conflict`, com uma mensagem de conflito e até três sugestões de horários disponíveis.

As sugestões mantêm a duração solicitada. A quantidade e os horários dependem da disponibilidade da quadra.

No próximo passo, utilize o ID da reserva criada com sucesso no passo 3.

### 5. Registrar o pagamento

Abra o Scalar de **Pagamentos** e execute `POST /api/pagamentos`.

Substitua o texto pelo ID da reserva criada anteriormente.

```json
{
  "reservaId": "COLE_O_ID_DA_RESERVA",
  "valor": 120,
  "formaPagamento": "Pix"
}
```

O resultado esperado é `201 Created`, com os seguintes valores.

- `status` igual a `Aprovado`.
- `reservaConfirmada` igual a `true`.

Copie também o campo `id` do pagamento.

Durante essa operação, Pagamentos consulta a reserva para verificar seu status e comparar o valor informado com o `valorTotal`.

Após aprovar o pagamento simulado, Pagamentos envia um `PATCH /api/reservas/{id}/confirmar-pagamento` com o ID do pagamento para confirmar a reserva.

### 6. Consultar a reserva confirmada

Volte ao Scalar de **Reservas** e execute `GET /api/reservas/{id}`.

Preencha o parâmetro `id` com o ID da reserva criada no passo 3.

O resultado esperado é `200 OK`, com os seguintes valores.

- `status` igual a `Confirmada`.
- `pagamentoId` igual ao ID do pagamento aprovado no passo 5.

Essa consulta permite observar a alteração realizada em Reservas a partir da chamada de Pagamentos.

### 7. Testes adicionais de validação

| Teste | Como executar | Resultado esperado |
|---|---|---|
| Valor incorreto | Crie outra reserva em um horário disponível e tente pagar com um valor diferente do seu `valorTotal`. | `400 Bad Request`. |
| Pagamento duplicado | Envie novamente o pedido do pagamento já aprovado no passo 5. | `409 Conflict`. |
| Pagamento recusado | Crie outra reserva em um horário disponível e envie o pagamento com o valor correto, incluindo `"simularRecusa": true` no corpo. | `201 Created`, pagamento `Recusado`, `reservaConfirmada: false` e reserva ainda `AguardandoPagamento`. |

### 8. Acompanhar as chamadas nos logs

Na requisição de criação da reserva ou de pagamento, adicione um cabeçalho no Scalar.

| Key | Value |
|---|---|
| `X-Correlation-Id` | `demonstracao-001` |

No Render, abra os logs dos serviços envolvidos e procure o mesmo valor.

Ao criar a reserva, observe os GETs recebidos por Clientes e Quadras. Ao registrar o pagamento, observe o GET de consulta e o PATCH de confirmação recebidos por Reservas.

## Execução local

Instale o SDK do .NET 10. Na raiz do repositório, abra um terminal por serviço e execute o comando correspondente.

| Serviço | Comando |
|---|---|
| Clientes | `dotnet run --project src/Clientes/Clientes.csproj --no-launch-profile -- --urls http://localhost:5101` |
| Quadras | `dotnet run --project src/Quadras/Quadras.csproj --no-launch-profile -- --urls http://localhost:5102` |
| Reservas | `dotnet run --project src/Reservas/Reservas.csproj --no-launch-profile -- --urls http://localhost:5103` |
| Pagamentos | `dotnet run --project src/Pagamentos/Pagamentos.csproj --no-launch-profile -- --urls http://localhost:5104` |

Cada serviço aplica as migrações do SQLite ao iniciar.

Após iniciar as APIs, acesse o Scalar local.

| Serviço | Endereço |
|---|---|
| Clientes | http://localhost:5101/scalar |
| Quadras | http://localhost:5102/scalar |
| Reservas | http://localhost:5103/scalar |
| Pagamentos | http://localhost:5104/scalar |

## Validação realizada

O fluxo completo foi testado no Render, incluindo cadastro de cliente e quadra, criação da reserva, aprovação do pagamento simulado e consulta da reserva com status `Confirmada`.

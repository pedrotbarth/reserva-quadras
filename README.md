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

## Testes pelo Scalar

Use as páginas abaixo para executar as requisições pelo navegador.

| Serviço | Scalar |
|---|---|
| Clientes | https://reserva-quadras-clientes.onrender.com/scalar |
| Quadras | https://reserva-quadras-quadras.onrender.com/scalar |
| Reservas | https://reserva-quadras-reservas.onrender.com/scalar |
| Pagamentos | https://reserva-quadras-pagamentos.onrender.com/scalar |

Abra os quatro links de saúde da seção de publicação e aguarde as respostas antes de começar. No Scalar, escolha a operação, clique em **Test Request**, preencha o **Request Body** e clique em **Send**.

Confira o endereço da requisição dentro do Scalar. No Render, ele deve começar com **https://**. Se aparecer **http://**, altere para **https://** antes de enviar. Esse ajuste evita falhas de envio pelo navegador.

Os exemplos criam dados de demonstração. Use os IDs retornados nesta execução. Para repetir o roteiro, cadastre um cliente e uma quadra novos. Na reserva, escolha uma data futura, por exemplo, o dia seguinte, no formato `AAAA-MM-DD`.

### 1. Cadastrar o cliente

Em **Clientes**, execute `POST /api/clientes`.

```json
{
  "nome": "Cliente Demonstracao",
  "telefone": "48999999999",
  "email": "cliente.demonstracao@example.com"
}
```

Resultado esperado: **201 Created**. Copie o campo `id` da resposta para usar como `clienteId`.

### 2. Cadastrar a quadra

Em **Quadras**, execute `POST /api/quadras`.

```json
{
  "nome": "Quadra Demonstracao",
  "modalidade": "Futsal",
  "valorPorHora": 120,
  "horarioAbertura": "08:00:00",
  "horarioFechamento": "22:00:00"
}
```

Resultado esperado: **201 Created**. Copie o campo `id` da resposta para usar como `quadraId`.

### 3. Criar a reserva e demonstrar as consultas entre serviços

Em **Reservas**, execute `POST /api/reservas`. Substitua os textos dos IDs e ajuste a data antes de enviar.

```json
{
  "clienteId": "COLE_O_ID_DO_CLIENTE",
  "quadraId": "COLE_O_ID_DA_QUADRA",
  "data": "2026-11-26",
  "horarioInicio": "19:00:00",
  "horarioFim": "20:00:00"
}
```

Resultado esperado: **201 Created**, `status: "AguardandoPagamento"` e `valorTotal: 120`. Copie o `id` da reserva.

Durante essa operação, Reservas consulta `GET /api/clientes/{id}` e `GET /api/quadras/{id}` para validar os cadastros, conferir o funcionamento e calcular o valor.

### 4. Demonstrar o conflito de horário

Envie novamente o mesmo pedido do passo 3, mantendo cliente, quadra, data e horários.

Resultado esperado: **409 Conflict**, com uma mensagem e até três sugestões de horários livres. A quantidade e os horários dependem da disponibilidade da quadra.

Use no pagamento o ID da reserva criada com sucesso no passo 3.

### 5. Registrar o pagamento e demonstrar a alteração entre serviços

Em **Pagamentos**, execute `POST /api/pagamentos` com o ID da reserva.

```json
{
  "reservaId": "COLE_O_ID_DA_RESERVA",
  "valor": 120,
  "formaPagamento": "Pix"
}
```

Resultado esperado: **201 Created**, `status: "Aprovado"` e `reservaConfirmada: true`. Copie também o `id` do pagamento.

Pagamentos consulta `GET /api/reservas/{id}` e compara o valor informado com o `valorTotal` da reserva. Após aprovar o pagamento simulado, chama `PATCH /api/reservas/{id}/confirmar-pagamento`, enviando o `pagamentoId`.

### 6. Conferir o estado final

Em **Reservas**, execute `GET /api/reservas/{id}`. Preencha o parâmetro `id` com o ID do passo 3.

Resultado esperado: **200 OK**, `status: "Confirmada"` e `pagamentoId` igual ao ID do pagamento aprovado no passo 5.

### 7. Testes adicionais de validação

| Teste | Como executar | Resultado esperado |
|---|---|---|
| Valor incorreto | Crie outra reserva em um horário livre e tente pagar com um valor diferente do seu `valorTotal`. | **400 Bad Request**. |
| Pagamento duplicado | Reenvie o pagamento aprovado do passo 5. | **409 Conflict**. |
| Pagamento recusado | Crie outra reserva em um horário livre e pague o valor correto, incluindo `"simularRecusa": true` no JSON do pagamento. | **201 Created**, pagamento `Recusado`, `reservaConfirmada: false` e reserva ainda `AguardandoPagamento`. |

### Acompanhar as chamadas nos logs

No pedido de criação de reserva ou pagamento, adicione o cabeçalho `X-Correlation-Id` com um valor como `demonstracao-001`. No Render, abra os logs dos serviços envolvidos e procure esse mesmo valor.

Na criação da reserva, observe os GETs recebidos por Clientes e Quadras. No pagamento, observe o GET de consulta e o PATCH de confirmação recebidos por Reservas.

O fluxo completo foi testado no Render. O pagamento retornou `Aprovado` e a reserva ficou com status `Confirmada`.

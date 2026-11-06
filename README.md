# Sistema de Reserva de Quadras Esportivas

**Integrantes:** Pedro Henrique Teixeira Barth, Marcelo Vinicius Batista Candido e João Vitor Goulart Viana

## Propósito

O sistema permite cadastrar clientes e quadras esportivas, reservar horários
e confirmar reservas por meio de pagamentos simulados. Quando houver conflito
de horário, a API sugere alternativas disponíveis.

## Usuários

- Cliente: solicita e consulta reservas e registra o pagamento simulado.
- Administrador: gerencia os cadastros de clientes e quadras e acompanha as reservas.

## Requisitos funcionais

1. Cadastrar, listar, consultar e editar clientes.
2. Cadastrar, listar, consultar e editar quadras, com modalidade, valor por hora,
   horário de abertura e fechamento e situação ativa ou inativa.
3. Criar, listar, consultar e cancelar reservas.
4. Ao criar uma reserva, consultar os serviços de Clientes e Quadras para
   validar os cadastros, verificar o horário de funcionamento e calcular o valor.
5. Impedir a sobreposição de reservas ativas para a mesma quadra.
6. Em caso de conflito, sugerir até três horários livres na mesma quadra e no
   mesmo dia, mantendo a duração solicitada.
7. Registrar pagamentos simulados e impedir pagamentos duplicados para a
   mesma reserva.
8. Ao aprovar o pagamento, solicitar ao serviço de Reservas que altere o
   status de Aguardando pagamento para Confirmada.

## Descrição técnica

| Microsserviço | Responsabilidade |
|---|---|
| Clientes | Gerencia o cadastro e a situação dos clientes. |
| Quadras | Gerencia o cadastro, os preços e os horários de funcionamento das quadras. |
| Reservas | Valida cliente e quadra, controla os horários e sugere alternativas. |
| Pagamentos | Registra o pagamento simulado e solicita a confirmação da reserva. |

Os quatro serviços seguirão o template do professor com ASP.NET Core,
Entity Framework Core, SQLite e APIs REST. Cada serviço terá seu próprio banco.

O serviço de Reservas consultará Clientes e Quadras via HTTP. O serviço de
Pagamentos chamará Reservas via HTTP para confirmar a reserva.

Cada serviço terá um endpoint /health para verificar o acesso ao SQLite.
Os logs registrarão o resultado das operações e o tempo das chamadas entre
serviços, com um identificador para acompanhar a mesma requisição.

Os quatro serviços serão publicados no Render. O código e os links ficarão
no GitHub.
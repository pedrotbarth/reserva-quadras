namespace Pagamentos.Servicos;

public class PagamentoDuplicadoException(Guid reservaId)
    : ErroPagamentoException(
        StatusCodes.Status409Conflict,
        "Pagamento duplicado.",
        $"A reserva {reservaId} já possui um pagamento aprovado.");

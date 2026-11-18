namespace Pagamentos.Servicos;

public class ReservasIndisponivelException()
    : ErroPagamentoException(
        StatusCodes.Status503ServiceUnavailable,
        "Serviço de Reservas indisponível.",
        "O serviço de Reservas não respondeu. Tente novamente em instantes.");

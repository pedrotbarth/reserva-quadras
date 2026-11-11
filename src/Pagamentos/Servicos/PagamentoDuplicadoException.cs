namespace Pagamentos.Servicos;

public class PagamentoDuplicadoException(Guid reservaId)
    : Exception($"A reserva {reservaId} já possui um pagamento aprovado.");

namespace Pagamentos.Servicos;

// Erro de regra de negócio que o controller transforma em resposta HTTP com o status indicado.
public class ErroPagamentoException(int statusCode, string titulo, string mensagem) : Exception(mensagem)
{
    public int StatusCode { get; } = statusCode;
    public string Titulo { get; } = titulo;
}

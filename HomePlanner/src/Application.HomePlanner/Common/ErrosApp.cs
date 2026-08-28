namespace Application.HomePlanner.Common;

/// <summary>
/// Catálogo de erros da aplicação. Cada entrada define o <b>código</b> (contrato com os
/// clientes) e o <b>texto padrão</b> em português (rede de segurança quando não há
/// tradução).
///
/// Ao acrescentar um erro aqui, crie também a chave <c>Erro_{codigo}</c> nos quatro
/// .resx — o teste <c>ErrosAppTests</c> falha se faltar alguma.
///
/// Só entram aqui erros que o usuário pode ver. Diagnóstico interno (configuração
/// ausente, tenant inexistente) continua como texto solto: traduzir isso para francês
/// não ajuda ninguém, e o texto só chega a quem está depurando.
/// </summary>
public static class ErrosApp
{
    // ── Sessão e autenticação ────────────────────────────────────────────────
    public static ErroOperacao SessaoInvalida => ErroOperacao.De(
        "sessao_invalida", "Sessão inválida. Faça login novamente.");

    public static ErroOperacao SessaoExpirada => ErroOperacao.De(
        "sessao_expirada", "Sessão expirada. Entre novamente.");

    public static ErroOperacao CredenciaisInvalidas => ErroOperacao.De(
        "credenciais_invalidas", "E-mail ou senha inválidos.");

    public static ErroOperacao ContaBloqueadaTentativas => ErroOperacao.De(
        "conta_bloqueada_tentativas",
        "Conta temporariamente bloqueada após várias tentativas. Tente novamente em 15 minutos.");

    public static ErroOperacao CodigoDoisFatoresInvalido => ErroOperacao.De(
        "codigo_2fa_invalido", "Código inválido. Verifique o app autenticador e tente novamente.");

    public static ErroOperacao LinkExpirado => ErroOperacao.De(
        "link_expirado", "Link inválido ou expirado. Peça um novo.");

    public static ErroOperacao EmailJaCadastrado => ErroOperacao.De(
        "email_ja_cadastrado", "Já existe uma conta com este e-mail.");

    public static ErroOperacao TermosNaoAceitos => ErroOperacao.De(
        "termos_nao_aceitos", "É preciso aceitar os termos de uso e a política de privacidade.");

    public static ErroOperacao EmailJaConfirmado => ErroOperacao.De(
        "email_ja_confirmado", "Seu e-mail já está confirmado.");

    public static ErroOperacao AtiveDoisFatoresPrimeiro => ErroOperacao.De(
        "ative_2fa_primeiro", "Ative a verificação em duas etapas primeiro.");

    // ── Validação de entrada ─────────────────────────────────────────────────
    public static ErroOperacao InformeNome => ErroOperacao.De(
        "informe_nome", "Informe seu nome.");

    public static ErroOperacao InformeEmail => ErroOperacao.De(
        "informe_email", "Informe um e-mail válido.");

    public static ErroOperacao IdiomaNaoSuportado => ErroOperacao.De(
        "idioma_nao_suportado", "Idioma não suportado.");

    /// <summary>{0} = nome do campo, {1} = mínimo de caracteres.</summary>
    public static ErroOperacao MuitoCurto(string campo, int minimo) => ErroOperacao.De(
        "muito_curto", $"{campo} deve ter pelo menos {minimo} caracteres.", campo, minimo);

    public static ErroOperacao DataDeveSerSegunda => ErroOperacao.De(
        "data_deve_ser_segunda", "A data de início deve ser uma segunda-feira.");

    // ── Permissão ────────────────────────────────────────────────────────────
    public static ErroOperacao SemPermissaoListas => ErroOperacao.De(
        "sem_permissao_listas", "Você não tem permissão para gerenciar listas.");

    public static ErroOperacao SemPermissaoRecorrentes => ErroOperacao.De(
        "sem_permissao_recorrentes", "Você não tem permissão para gerenciar produtos recorrentes.");

    public static ErroOperacao SomenteTarefasProprias => ErroOperacao.De(
        "somente_proprias_tarefas", "Você só pode alterar as suas próprias tarefas.");

    public static ErroOperacao SomentePedidosProprios => ErroOperacao.De(
        "somente_proprios_pedidos", "Você só pode alterar os seus próprios pedidos.");

    // ── Não encontrado (gênero muda por recurso, por isso separados) ─────────
    public static ErroOperacao ReceitaNaoEncontrada => ErroOperacao.De(
        "receita_nao_encontrada", "Receita não encontrada.");

    public static ErroOperacao IngredienteNaoEncontrado => ErroOperacao.De(
        "ingrediente_nao_encontrado", "Ingrediente não encontrado.");

    public static ErroOperacao MembroNaoEncontrado => ErroOperacao.De(
        "membro_nao_encontrado", "Membro não encontrado.");

    public static ErroOperacao ListaNaoEncontrada => ErroOperacao.De(
        "lista_nao_encontrada", "Lista não encontrada.");

    public static ErroOperacao PedidoNaoEncontrado => ErroOperacao.De(
        "pedido_nao_encontrado", "Pedido não encontrado.");

    public static ErroOperacao TarefaNaoEncontrada => ErroOperacao.De(
        "tarefa_nao_encontrada", "Tarefa não encontrada.");

    public static ErroOperacao ModeloNaoEncontrado => ErroOperacao.De(
        "modelo_nao_encontrado", "Modelo não encontrado.");

    public static ErroOperacao CardapioNaoEncontrado => ErroOperacao.De(
        "cardapio_nao_encontrado", "Cardápio não encontrado.");

    public static ErroOperacao SemanaNaoEncontrada => ErroOperacao.De(
        "semana_nao_encontrada", "Semana não encontrada.");

    public static ErroOperacao ProdutoNaoEncontrado => ErroOperacao.De(
        "produto_nao_encontrado", "Produto não encontrado.");

    // ── Família ──────────────────────────────────────────────────────────────
    public static ErroOperacao OwnerNaoPodeSerDesativado => ErroOperacao.De(
        "owner_nao_pode_desativar", "O administrador (Owner) não pode ser desativado.");

    public static ErroOperacao OwnerNaoPodeSerRemovido => ErroOperacao.De(
        "owner_nao_pode_remover", "O administrador (Owner) não pode ser removido.");

    public static ErroOperacao OwnerPapelImutavel => ErroOperacao.De(
        "owner_papel_imutavel", "O papel do administrador (Owner) não pode ser alterado.");

    public static ErroOperacao MembroJaAtivouRemocao => ErroOperacao.De(
        "membro_ja_ativou_remocao",
        "Este membro já ativou a conta e pode ter dados no sistema. Use Desativar em vez de remover.");

    public static ErroOperacao MembroJaAtivouEmail => ErroOperacao.De(
        "membro_ja_ativou_email",
        "Este membro já ativou a conta. O e-mail não pode mais ser alterado; use Desativar se necessário.");

    /// <summary>{0} = limite de membros do plano.</summary>
    public static ErroOperacao LimiteMembrosPlano(int limite) => ErroOperacao.De(
        "limite_membros_plano", $"Seu plano atual permite até {limite} membro(s) ativo(s).", limite);

    /// <summary>{0} = segundos a aguardar.</summary>
    public static ErroOperacao ConviteAguardeReenvio(int segundos) => ErroOperacao.De(
        "convite_aguarde_reenvio",
        $"O convite acabou de ser enviado. Aguarde {segundos} segundo(s) para reenviar.", segundos);

    // ── Assinatura ───────────────────────────────────────────────────────────
    public static ErroOperacao SemAssinaturaAtiva => ErroOperacao.De(
        "sem_assinatura_ativa", "Você ainda não tem uma assinatura ativa. Contrate um plano primeiro.");

    /// <summary>{0} = limite de importações do plano.</summary>
    public static ErroOperacao LimiteImportacoes(int limite) => ErroOperacao.De(
        "limite_importacoes",
        $"Você atingiu o limite de {limite} importações de receita este mês no seu plano.", limite);

    // ── Imagem ───────────────────────────────────────────────────────────────
    public static ErroOperacao ImagemFormatoInvalido => ErroOperacao.De(
        "imagem_formato_invalido", "Formato de imagem não suportado. Use JPEG, PNG ou WebP.");

    public static ErroOperacao ImagemCorrompida => ErroOperacao.De(
        "imagem_corrompida", "Imagem inválida ou corrompida.");

    public static ErroOperacao ImagemMuitoGrande => ErroOperacao.De(
        "imagem_muito_grande", "A imagem excede o tamanho máximo permitido (2 MB).");

    public static ErroOperacao ArquivoVazio => ErroOperacao.De(
        "arquivo_vazio", "Arquivo vazio.");

    // ── Importação e IA ──────────────────────────────────────────────────────
    public static ErroOperacao UrlInvalida => ErroOperacao.De(
        "url_invalida", "URL inválida.");

    public static ErroOperacao ReceitaNaoIdentificada => ErroOperacao.De(
        "receita_nao_identificada", "Não foi possível identificar uma receita nesta página.");

    public static ErroOperacao MuitasAnalisesIA => ErroOperacao.De(
        "muitas_analises_ia",
        "Muitas análises de ingredientes em pouco tempo. Aguarde alguns minutos e tente novamente.");

    // ── Envio de mensagens ───────────────────────────────────────────────────
    public static ErroOperacao EnvioIndisponivel => ErroOperacao.De(
        "envio_indisponivel", "Não conseguimos enviar sua mensagem agora. Tente novamente em instantes.");

    // ── Validação de formulário ──────────────────────────────────────────────
    public static ErroOperacao AntecedenciaInvalida => ErroOperacao.De(
        "antecedencia_invalida", "A antecedência do lembrete deve estar entre 0 e 1440 minutos.");

    public static ErroOperacao DestinoDeveSerSegunda => ErroOperacao.De(
        "destino_deve_ser_segunda", "O destino deve ser uma segunda-feira.");

    public static ErroOperacao HoraFimAntesDoInicio => ErroOperacao.De(
        "hora_fim_antes_inicio", "A hora de fim deve ser igual ou posterior à hora de início.");

    public static ErroOperacao PorcoesMinimo => ErroOperacao.De(
        "porcoes_minimo", "Número de porções deve ser ao menos 1.");

    public static ErroOperacao TamanhoFamiliaMinimo => ErroOperacao.De(
        "tamanho_familia_minimo", "O tamanho da família deve ser ao menos 1.");

    public static ErroOperacao SelecioneTipoRefeicao => ErroOperacao.De(
        "selecione_tipo_refeicao", "Selecione ao menos um tipo de refeição.");

    public static ErroOperacao InformeNomeMembro => ErroOperacao.De(
        "informe_nome_membro", "Informe o nome do membro.");

    public static ErroOperacao NomeReceitaCurto => ErroOperacao.De(
        "nome_receita_curto", "Nome da receita deve ter pelo menos 2 caracteres.");

    public static ErroOperacao NomeIngredienteCurto => ErroOperacao.De(
        "nome_ingrediente_curto", "Nome do ingrediente deve ter pelo menos 2 caracteres.");

    public static ErroOperacao NomeModeloCurto => ErroOperacao.De(
        "nome_modelo_curto", "Nome do modelo deve ter pelo menos 2 caracteres.");

    public static ErroOperacao NomeListaCurto => ErroOperacao.De(
        "nome_lista_curto", "O nome da lista deve ter pelo menos 2 caracteres.");

    public static ErroOperacao TituloTarefaCurto => ErroOperacao.De(
        "titulo_tarefa_curto", "O título da tarefa deve ter pelo menos 2 caracteres.");

    public static ErroOperacao DescricaoPedidoCurta => ErroOperacao.De(
        "descricao_pedido_curta", "A descrição do pedido deve ter pelo menos 2 caracteres.");

    public static ErroOperacao DescricaoProdutoCurta => ErroOperacao.De(
        "descricao_produto_curta", "A descrição do produto deve ter pelo menos 2 caracteres.");

    // ── Família (papéis e convites) ──────────────────────────────────────────
    public static ErroOperacao MembroJaAtivouSenha => ErroOperacao.De(
        "membro_ja_ativou_senha",
        "Este membro já ativou a conta. Ele pode redefinir a senha pela tela de login.");

    public static ErroOperacao OwnerDadosNoPerfil => ErroOperacao.De(
        "owner_dados_no_perfil", "Os dados do administrador (Owner) devem ser alterados no perfil.");

    public static ErroOperacao PapelInvalido => ErroOperacao.De(
        "papel_invalido", "Papel inválido para um membro.");

    public static ErroOperacao SomenteAdminAlteraPapel => ErroOperacao.De(
        "somente_admin_altera_papel", "Somente o administrador pode alterar o papel de um membro.");

    public static ErroOperacao SoAlternaPaiFilho => ErroOperacao.De(
        "so_alterna_pai_filho", "Só é possível alternar entre Pai e Filho.");

    public static ErroOperacao ReativeAntesDeReenviar => ErroOperacao.De(
        "reative_antes_reenviar", "Reative o acesso deste membro antes de reenviar o convite.");

    public static ErroOperacao UsuarioNaoEncontrado => ErroOperacao.De(
        "usuario_nao_encontrado", "Usuário não encontrado.");

    // ── Receitas e ingredientes ──────────────────────────────────────────────
    public static ErroOperacao CicloComponente => ErroOperacao.De(
        "ciclo_componente", "Esse componente criaria um ciclo (ele já usa este prato).");

    public static ErroOperacao CicloAgrupamento => ErroOperacao.De(
        "ciclo_agrupamento", "Este agrupamento criaria um ciclo.");

    public static ErroOperacao ReceitaComponenteDeSi => ErroOperacao.De(
        "receita_componente_de_si", "Uma receita não pode ser componente de si mesma.");

    public static ErroOperacao IngredienteBaseDeSi => ErroOperacao.De(
        "ingrediente_base_de_si", "Um ingrediente não pode ser produto base de si mesmo.");

    public static ErroOperacao ProdutoBaseNaoEncontrado => ErroOperacao.De(
        "produto_base_nao_encontrado", "Produto base não encontrado.");

    /// <summary>{0} = nome do ingrediente.</summary>
    public static ErroOperacao IngredienteDuplicado(string nome) => ErroOperacao.De(
        "ingrediente_duplicado", $"Já existe um ingrediente com o nome '{nome}'.", nome);

    /// <summary>{0} = receitas que usam esta como componente.</summary>
    public static ErroOperacao ReceitaUsadaComoComponente(string receitas) => ErroOperacao.De(
        "receita_usada_componente", $"Esta receita é usada como componente em: {receitas}", receitas);

    /// <summary>{0} = id da unidade.</summary>
    public static ErroOperacao UnidadeNaoEncontrada(object id) => ErroOperacao.De(
        "unidade_nao_encontrada", $"Unidade de medida {id} não encontrada.", id);

    /// <summary>{0} e {1} = códigos das unidades.</summary>
    public static ErroOperacao UnidadesIncompativeis(string origem, string destino) => ErroOperacao.De(
        "unidades_incompativeis",
        $"Unidades incompatíveis: {origem} e {destino} são de tipos diferentes.", origem, destino);

    // ── Cardápio e compras ───────────────────────────────────────────────────
    public static ErroOperacao SemanaOrigemNaoEncontrada => ErroOperacao.De(
        "semana_origem_nao_encontrada", "Semana de origem não encontrada.");

    public static ErroOperacao SemanaSemCardapio => ErroOperacao.De(
        "semana_sem_cardapio", "Semana não encontrada. Crie o cardápio primeiro.");

    public static ErroOperacao FalhaLimparMarcacoes => ErroOperacao.De(
        "falha_limpar_marcacoes", "Não foi possível limpar as marcações.");

    public static ErroOperacao FalhaSalvarMarcacao => ErroOperacao.De(
        "falha_salvar_marcacao", "Não foi possível salvar a marcação.");

    // ── Imagem ───────────────────────────────────────────────────────────────
    public static ErroOperacao ImagemFormatoInvalidoUpload => ErroOperacao.De(
        "imagem_formato_invalido_upload", "Formato inválido. Use JPG, PNG, WEBP ou GIF.");

    public static ErroOperacao ImagemErroProcessar => ErroOperacao.De(
        "imagem_erro_processar", "Erro ao processar a imagem. Tente outra.");

    public static ErroOperacao ImagemNaoLegivel => ErroOperacao.De(
        "imagem_nao_legivel", "Não foi possível ler a imagem. Verifique se o arquivo está correto.");

    /// <summary>{0} = limite em MB.</summary>
    public static ErroOperacao ImagemEntradaMuitoGrande(object mb) => ErroOperacao.De(
        "imagem_entrada_muito_grande", $"Imagem muito grande. Máximo {mb} MB.", mb);

    /// <summary>{0} = tamanho final em KB.</summary>
    public static ErroOperacao ImagemFinalAcimaDoLimite(object kb) => ErroOperacao.De(
        "imagem_final_acima_limite", $"A foto final ainda está acima do limite ({kb} KB).", kb);

    // ── Importação ───────────────────────────────────────────────────────────
    public static ErroOperacao UrlNaoInformada => ErroOperacao.De(
        "url_nao_informada", "URL não informada.");

    /// <summary>{0} = detalhe do erro de rede.</summary>
    public static ErroOperacao UrlInacessivel(string detalhe) => ErroOperacao.De(
        "url_inacessivel", $"Não foi possível acessar a URL: {detalhe}", detalhe);

    // ── Assinatura e cadastro ────────────────────────────────────────────────
    public static ErroOperacao CheckoutFalhou => ErroOperacao.De(
        "checkout_falhou", "Não foi possível iniciar o checkout. Tente novamente.");

    public static ErroOperacao PortalFalhou => ErroOperacao.De(
        "portal_falhou", "Não foi possível abrir o gerenciamento. Tente novamente.");

    public static ErroOperacao CadastroFalhou => ErroOperacao.De(
        "cadastro_falhou", "Não foi possível concluir o cadastro. Tente novamente.");

    /// <summary>
    /// Todos os códigos declarados nesta classe — usado pelo teste que garante tradução
    /// para cada um. Vem por reflexão de propósito: uma lista à mão dessincroniza no dia
    /// em que alguém acrescenta um erro e esquece de registrá-la, que é exatamente o caso
    /// que o teste deveria pegar.
    ///
    /// As propriedades sem argumento são lidas direto; os erros com parâmetro são métodos
    /// e precisam de valores de exemplo, por isso vêm da lista abaixo.
    /// </summary>
    public static IReadOnlyList<string> TodosOsCodigos =>
        typeof(ErrosApp)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(ErroOperacao))
            .Select(p => ((ErroOperacao)p.GetValue(null)!).Codigo)
            .Concat(CodigosComParametro)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

    /// <summary>Códigos cujos erros são métodos (recebem argumentos).</summary>
    private static readonly string[] CodigosComParametro =
    [
        "muito_curto", "limite_membros_plano", "convite_aguarde_reenvio", "limite_importacoes",
        "ingrediente_duplicado", "receita_usada_componente", "unidade_nao_encontrada",
        "unidades_incompativeis", "imagem_entrada_muito_grande", "imagem_final_acima_limite",
        "url_inacessivel",
    ];
}

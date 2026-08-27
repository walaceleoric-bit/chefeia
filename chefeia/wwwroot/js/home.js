document.addEventListener("DOMContentLoaded", () => {

    const ingredienteIA =
        document.getElementById("ingredienteIA");

    const btnAdicionarIngrediente =
        document.getElementById("btnAdicionarIngrediente");

    const listaIngredientes =
        document.getElementById("listaIngredientes");

    const btnDescobrirIA =
        document.getElementById("btnDescobrirIA");

    const preferenciaIA =
        document.getElementById("preferenciaIA");

    const porcoesIA =
        document.getElementById("porcoesIA");

    const statusIA =
        document.getElementById("statusIA");

    const resultadoIA =
        document.getElementById("resultadoIA");

    const orientacaoIA =
        document.getElementById("orientacaoIA");

    const tituloOrientacaoIA =
        document.getElementById("tituloOrientacaoIA");

    const subtituloOrientacaoIA =
        document.getElementById("subtituloOrientacaoIA");

    const mensagemOrientacaoIA =
        document.getElementById("mensagemOrientacaoIA");

    const blocoSugestoesIA =
        document.getElementById("blocoSugestoesIA");

    const sugestoesIA =
        document.getElementById("sugestoesIA");


    const ingredientesSelecionados = [];


    // =====================================================
    // ADICIONAR INGREDIENTE
    // =====================================================

    if (
        btnAdicionarIngrediente &&
        ingredienteIA
    ) {
        btnAdicionarIngrediente.addEventListener(
            "click",
            adicionarIngrediente
        );

        ingredienteIA.addEventListener(
            "keydown",
            event => {
                if (event.key === "Enter") {
                    event.preventDefault();
                    adicionarIngrediente();
                }
            }
        );
    }


    function adicionarIngrediente() {

        if (!ingredienteIA) {
            return;
        }

        const ingrediente =
            ingredienteIA.value.trim();

        if (!ingrediente) {

            if (statusIA) {
                statusIA.innerHTML =
                    mensagemAviso(
                        "Digite um ingrediente."
                    );
            }

            return;
        }

        const jaExiste =
            ingredientesSelecionados.some(
                item =>
                    item.toLowerCase() ===
                    ingrediente.toLowerCase()
            );

        if (jaExiste) {

            if (statusIA) {
                statusIA.innerHTML =
                    mensagemAviso(
                        "Esse ingrediente já foi adicionado."
                    );
            }

            return;
        }

        ingredientesSelecionados.push(
            ingrediente
        );

        ingredienteIA.value = "";

        ingredienteIA.focus();

        if (statusIA) {
            statusIA.innerHTML = "";
        }

        atualizarListaIngredientes();
    }


    // =====================================================
    // LISTA DE INGREDIENTES
    // =====================================================

    function atualizarListaIngredientes() {

        if (!listaIngredientes) {
            return;
        }

        listaIngredientes.innerHTML =
            ingredientesSelecionados
                .map(
                    (ingrediente, indice) => {
                        return `
                            <span class="ingrediente-tag">

                                ${escapeHtml(ingrediente)}

                                <button
                                    type="button"
                                    class="remover-ingrediente"
                                    data-indice="${indice}"
                                    title="Remover ingrediente">

                                    ×

                                </button>

                            </span>
                        `;
                    }
                )
                .join("");

        const botoesRemover =
            document.querySelectorAll(
                ".remover-ingrediente"
            );

        botoesRemover.forEach(
            botaoRemover => {

                botaoRemover.addEventListener(
                    "click",
                    () => {

                        const indice =
                            Number(
                                botaoRemover
                                    .dataset
                                    .indice
                            );

                        ingredientesSelecionados.splice(
                            indice,
                            1
                        );

                        atualizarListaIngredientes();
                    }
                );
            }
        );
    }


    // =====================================================
    // BOTÃO IA
    // =====================================================

    if (btnDescobrirIA) {

        btnDescobrirIA.addEventListener(
            "click",
            consultarChefeIA
        );
    }


    // =====================================================
    // CONSULTAR IA
    // =====================================================

    async function consultarChefeIA() {

        if (
            ingredientesSelecionados.length === 0
        ) {

            if (statusIA) {
                statusIA.innerHTML =
                    mensagemAviso(
                        "Adicione pelo menos um ingrediente."
                    );
            }

            return;
        }

        const porcoes =
            Number(
                porcoesIA?.value
            );

        const consulta =
        {
            ingredientes:
                ingredientesSelecionados,

            preferencia:
                preferenciaIA?.value || "",

            porcoes:
                porcoes > 0
                    ? porcoes
                    : 1
        };

        esconderResultados();

        if (statusIA) {

            statusIA.innerHTML =
                `
                <div style="
                    padding:12px;
                    color:#76573d;
                    font-size:13px;
                ">
                    👨‍🍳 O Chefe IA está analisando
                    seus ingredientes...
                </div>
                `;
        }

        if (!btnDescobrirIA) {
            return;
        }

        btnDescobrirIA.disabled = true;

        const textoOriginalBotao =
            btnDescobrirIA.innerHTML;

        btnDescobrirIA.innerHTML =
            "👨‍🍳 Analisando...";


        try {

            const response =
                await fetch(
                    "/api/ai/sugerir-receita",
                    {
                        method: "POST",

                        headers: {
                            "Content-Type":
                                "application/json"
                        },

                        body:
                            JSON.stringify(
                                consulta
                            )
                    }
                );


            // =================================================
            // NÃO LOGADO
            // =================================================

            if (
                response.status === 401
            ) {

                if (statusIA) {
                    statusIA.innerHTML =
                        mensagemLogin();
                }

                setTimeout(
                    () => {

                        const returnUrl =
                            encodeURIComponent(
                                window.location.pathname +
                                window.location.search
                            );

                        window.location.href =
                            `/Conta/Login?returnUrl=${returnUrl}`;
                    },
                    900
                );

                return;
            }


            // =================================================
            // CONTA DESATIVADA
            // =================================================

            if (
                response.status === 403
            ) {

                const dados =
                    await tentarLerJson(
                        response
                    );

                if (statusIA) {
                    statusIA.innerHTML =
                        mensagemErro(
                            dados?.message ||
                            "Sua conta não possui permissão para utilizar a IA."
                        );
                }

                return;
            }


            // =================================================
            // LIMITE DO PLANO DO USUÁRIO
            // =================================================

            if (
                response.status === 429
            ) {

                const dados =
                    await tentarLerJson(
                        response
                    );

                if (
                    dados?.limitReached === true &&
                    dados?.externalLimit !== true
                ) {
                    mostrarLimiteAtingido(
                        dados
                    );

                    return;
                }

                if (statusIA) {
                    statusIA.innerHTML =
                        mensagemErro(
                            dados?.message ||
                            "Não foi possível continuar a consulta."
                        );
                }

                return;
            }


            // =================================================
            // RAPIDAPI / SERVIÇO EXTERNO INDISPONÍVEL
            // =================================================

            if (
                response.status === 503
            ) {

                const dados =
                    await tentarLerJson(
                        response
                    );

                esconderResultados();

                if (statusIA) {

                    statusIA.innerHTML =
                        mensagemServicoIndisponivel(
                            dados?.message ||
                            "O serviço de inteligência artificial está temporariamente indisponível."
                        );
                }

                return;
            }


            // =================================================
            // OUTROS ERROS
            // =================================================

            if (!response.ok) {

                const dados =
                    await tentarLerJson(
                        response
                    );

                throw new Error(
                    dados?.message ||
                    `Erro HTTP: ${response.status}`
                );
            }


            // =================================================
            // RESPOSTA VÁLIDA
            // =================================================

            const dados =
                await response.json();

            if (
                !dados ||
                dados.success !== true
            ) {

                throw new Error(
                    "A resposta do servidor não é válida."
                );
            }


            // =================================================
            // RECEITA GERADA
            // =================================================

            if (
                dados.hasRecipe === true &&
                dados.recipe
            ) {

                mostrarReceitaIA(
                    dados.recipe
                );

                mostrarUsoPlano(
                    dados.usage
                );

                return;
            }


            // =================================================
            // OPINIÃO / SUGESTÃO
            // =================================================

            if (
                dados.hasRecipe === false
            ) {

                mostrarOrientacaoIA(
                    dados.responseType,
                    dados.message,
                    dados.suggestions
                );

                mostrarUsoPlano(
                    dados.usage
                );

                return;
            }


            throw new Error(
                "O servidor não informou um resultado válido."
            );
        }
        catch (erro) {

            console.error(
                "Erro ao consultar Chefe IA:",
                erro
            );

            esconderResultados();

            if (statusIA) {

                statusIA.innerHTML =
                    mensagemErro(
                        erro?.message ||
                        "Não foi possível obter uma sugestão agora."
                    );
            }
        }
        finally {

            btnDescobrirIA.disabled =
                false;

            btnDescobrirIA.innerHTML =
                textoOriginalBotao;
        }
    }


    // =====================================================
    // ESCONDER RESULTADOS
    // =====================================================

    function esconderResultados() {

        if (resultadoIA) {
            resultadoIA.style.display =
                "none";
        }

        if (orientacaoIA) {
            orientacaoIA.style.display =
                "none";
        }
    }


    // =====================================================
    // ORIENTAÇÃO DA IA
    // =====================================================

    function mostrarOrientacaoIA(
        tipoResposta,
        mensagem,
        sugestoes
    ) {

        if (resultadoIA) {
            resultadoIA.style.display =
                "none";
        }

        const tipo =
            String(
                tipoResposta ||
                "INSUFICIENTE"
            )
                .trim()
                .toUpperCase();

        if (tituloOrientacaoIA) {
            tituloOrientacaoIA.textContent =
                "👨‍🍳 OPINIÃO DO CHEFE IA";
        }

        if (subtituloOrientacaoIA) {

            if (
                tipo === "SUGESTAO"
            ) {
                subtituloOrientacaoIA.textContent =
                    "Dá para melhorar essa ideia";
            }
            else {
                subtituloOrientacaoIA.textContent =
                    "Eu não faria uma receita assim";
            }
        }

        if (mensagemOrientacaoIA) {

            mensagemOrientacaoIA.textContent =
                mensagem ||
                "Com os ingredientes informados, ainda não encontrei uma combinação que eu recomende.";
        }

        const listaSugestoes =
            Array.isArray(sugestoes)
                ? sugestoes.filter(
                    item =>
                        item &&
                        String(item).trim()
                )
                : [];

        if (
            blocoSugestoesIA &&
            sugestoesIA
        ) {

            if (
                listaSugestoes.length > 0
            ) {

                sugestoesIA.innerHTML =
                    listaSugestoes
                        .map(
                            sugestao =>
                                `<li>${escapeHtml(sugestao)}</li>`
                        )
                        .join("");

                blocoSugestoesIA.style.display =
                    "block";
            }
            else {

                sugestoesIA.innerHTML =
                    "";

                blocoSugestoesIA.style.display =
                    "none";
            }
        }

        if (orientacaoIA) {

            orientacaoIA.style.display =
                "block";

            orientacaoIA.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
        }
    }


    // =====================================================
    // USO DO PLANO
    // =====================================================

    function mostrarUsoPlano(usage) {

        if (!statusIA) {
            return;
        }

        if (!usage) {
            statusIA.innerHTML = "";
            return;
        }

        const premium =
            usage.plan === "PREMIUM";

        const icone =
            premium
                ? "👑"
                : "🌿";

        const nomePlano =
            usage.planName ||
            (
                premium
                    ? "Premium"
                    : "Gratuito"
            );

        statusIA.innerHTML =
            `
            <div style="
                margin-top:12px;
                padding:12px 15px;
                border-radius:12px;
                background:${premium ? "#fff4e8" : "#eff8ed"};
                color:${premium ? "#a94d14" : "#347b35"};
                font-size:13px;
                text-align:center;
            ">

                ${icone}
                Plano
                <strong>
                    ${escapeHtml(nomePlano)}
                </strong>

                ·

                <strong>
                    ${Number(usage.used || 0)}
                    /
                    ${Number(usage.limit || 0)}
                </strong>

                consultas usadas este mês

                ·

                <strong>
                    ${Number(usage.remaining || 0)}
                </strong>

                restante(s)

            </div>
            `;
    }


    // =====================================================
    // LIMITE INTERNO DO USUÁRIO
    // =====================================================

    function mostrarLimiteAtingido(
        dados
    ) {

        if (!statusIA) {
            return;
        }

        esconderResultados();

        const premium =
            dados?.plan === "PREMIUM";

        const limite =
            Number(
                dados?.limit || 0
            );

        const usadas =
            Number(
                dados?.used || limite
            );

        const mensagem =
            premium
                ? `
                    Você utilizou
                    <strong>
                        ${usadas}/${limite}
                    </strong>
                    consultas do seu plano Premium neste mês.
                  `
                : `
                    Você utilizou
                    <strong>
                        ${usadas}/${limite}
                    </strong>
                    consultas gratuitas deste mês.
                  `;

        const upgrade =
            premium
                ? ""
                : `
                    <div style="
                        margin-top:14px;
                    ">

                        <a
                            href="/Assinatura/Premium"
                            style="
                                display:inline-block;
                                padding:10px 15px;
                                border-radius:9px;
                                text-decoration:none;
                                background:#f5661b;
                                color:white;
                                font-weight:800;
                            ">

                            👑 Conhecer Premium

                        </a>

                    </div>
                  `;

        statusIA.innerHTML =
            `
            <div style="
                margin-top:12px;
                padding:18px;
                border-radius:14px;
                background:#fff0ee;
                border:1px solid #f3c8c2;
                color:#8f332b;
                text-align:center;
                line-height:1.55;
            ">

                <div style="
                    font-size:27px;
                    margin-bottom:7px;
                ">
                    🚫
                </div>

                <strong>
                    Limite mensal atingido
                </strong>

                <div style="
                    margin-top:6px;
                ">
                    ${mensagem}
                </div>

                <div style="
                    margin-top:6px;
                    font-size:12px;
                    color:#9e5a54;
                ">
                    O limite será renovado
                    automaticamente no próximo mês.
                </div>

                ${upgrade}

            </div>
            `;
    }


    // =====================================================
    // SERVIÇO EXTERNO INDISPONÍVEL
    // =====================================================

    function mensagemServicoIndisponivel(
        texto
    ) {

        return `
            <div style="
                margin-top:12px;
                padding:18px;
                border-radius:14px;
                background:#fff8e8;
                border:1px solid #ead8aa;
                color:#76573d;
                text-align:center;
                line-height:1.55;
            ">

                <div style="
                    font-size:28px;
                    margin-bottom:8px;
                ">
                    ⏳
                </div>

                <strong>
                    Chefe IA temporariamente indisponível
                </strong>

                <div style="
                    margin-top:7px;
                    font-size:13px;
                ">
                    ${escapeHtml(texto)}
                </div>

                <div style="
                    margin-top:8px;
                    font-size:12px;
                    opacity:.8;
                ">
                    Essa tentativa não foi contabilizada
                    como consulta utilizada.
                </div>

            </div>
        `;
    }


    // =====================================================
    // MOSTRAR RECEITA
    // =====================================================

    function mostrarReceitaIA(
        receita
    ) {

        if (orientacaoIA) {
            orientacaoIA.style.display =
                "none";
        }

        const pais =
            document.getElementById(
                "paisReceitaIA"
            );

        const nome =
            document.getElementById(
                "nomeReceitaIA"
            );

        const descricao =
            document.getElementById(
                "descricaoReceitaIA"
            );

        const categoria =
            document.getElementById(
                "categoriaReceitaIA"
            );

        const tempo =
            document.getElementById(
                "tempoReceitaIA"
            );

        const porcoes =
            document.getElementById(
                "porcoesReceitaIA"
            );

        const listaIngredientesReceita =
            document.getElementById(
                "ingredientesReceitaIA"
            );

        const listaPassos =
            document.getElementById(
                "passosReceitaIA"
            );


        if (pais) {

            pais.textContent =
                receita.pais
                    ? `🌎 ${receita.pais}`
                    : "";
        }

        if (nome) {

            nome.textContent =
                receita.nome ||
                "Receita sugerida";
        }

        if (descricao) {

            descricao.textContent =
                receita.descricao || "";
        }

        if (categoria) {

            categoria.textContent =
                receita.categoria
                    ? `🍽 ${receita.categoria}`
                    : "";
        }

        if (tempo) {

            tempo.textContent =
                receita.tempoMinutos
                    ? `⏱ ${receita.tempoMinutos} min`
                    : "";
        }

        if (porcoes) {

            porcoes.textContent =
                receita.porcoes
                    ? `👥 ${receita.porcoes} porções`
                    : "";
        }

        if (listaIngredientesReceita) {

            listaIngredientesReceita.innerHTML =
                (
                    receita.ingredientes ||
                    []
                )
                    .map(
                        ingrediente =>
                            `<li>${escapeHtml(ingrediente)}</li>`
                    )
                    .join("");
        }

        if (listaPassos) {

            listaPassos.innerHTML =
                (
                    receita.passos ||
                    []
                )
                    .map(
                        passo =>
                            `<li>${escapeHtml(passo)}</li>`
                    )
                    .join("");
        }

        if (resultadoIA) {

            resultadoIA.style.display =
                "block";

            resultadoIA.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
        }
    }


    // =====================================================
    // MENSAGENS
    // =====================================================

    function mensagemAviso(
        texto
    ) {

        return `
            <div style="
                margin-top:10px;
                padding:10px 12px;
                border-radius:10px;
                background:#fff8e8;
                color:#76573d;
                font-size:13px;
            ">
                ⚠️ ${escapeHtml(texto)}
            </div>
        `;
    }


    function mensagemErro(
        texto
    ) {

        return `
            <div style="
                margin-top:10px;
                padding:12px;
                border-radius:10px;
                background:#fff0ee;
                color:#a6382e;
                font-size:13px;
            ">
                ❌ ${escapeHtml(texto)}
            </div>
        `;
    }


    function mensagemLogin() {

        return `
            <div style="
                margin-top:10px;
                padding:14px;
                border-radius:12px;
                background:#fff4e8;
                color:#88441d;
                text-align:center;
                font-size:13px;
            ">

                🔐
                <strong>
                    Faça login para criar sua receita.
                </strong>

                <br />

                <span style="
                    font-size:12px;
                ">
                    Redirecionando...
                </span>

            </div>
        `;
    }


    // =====================================================
    // LER JSON
    // =====================================================

    async function tentarLerJson(
        response
    ) {

        try {
            return await response.json();
        }
        catch {
            return null;
        }
    }


    // =====================================================
    // SEGURANÇA HTML
    // =====================================================

    function escapeHtml(
        valor
    ) {

        if (
            valor === null ||
            valor === undefined
        ) {
            return "";
        }

        return String(valor)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

});
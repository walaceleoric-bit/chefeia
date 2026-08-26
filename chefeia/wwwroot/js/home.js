document.addEventListener("DOMContentLoaded", () => {

    // =====================================================
    // BUSCA DE RECEITAS PRONTAS
    // =====================================================

    const input =
        document.getElementById("ingredienteBusca");

    const botao =
        document.getElementById("btnBuscar");

    const resultadoBusca =
        document.getElementById("resultadoBusca");

    const secaoResultados =
        document.getElementById("secaoResultados");

    const receitasEncontradas =
        document.getElementById("receitasEncontradas");


    if (botao && input) {

        botao.addEventListener(
            "click",
            buscarReceitas
        );


        input.addEventListener(
            "keydown",
            event => {

                if (event.key === "Enter") {

                    event.preventDefault();

                    buscarReceitas();

                }

            }
        );

    }


    async function buscarReceitas() {

        const termo =
            input.value.trim();


        if (!termo) {

            resultadoBusca.innerHTML =
                "<p>Digite algo para pesquisar.</p>";

            return;

        }


        resultadoBusca.innerHTML =
            "<p>🔎 Buscando receitas...</p>";


        secaoResultados.style.display =
            "none";


        receitasEncontradas.innerHTML =
            "";


        try {

            const response =
                await fetch(
                    `/api/receitas/buscar?termo=${encodeURIComponent(termo)}`
                );


            if (!response.ok) {

                throw new Error(
                    "Erro ao consultar receitas."
                );

            }


            const receitas =
                await response.json();


            if (
                !Array.isArray(receitas) ||
                receitas.length === 0
            ) {

                resultadoBusca.innerHTML =
                    `
                    <p>
                        Nenhuma receita encontrada para
                        <strong>${escapeHtml(termo)}</strong>.
                    </p>
                    `;

                return;

            }


            resultadoBusca.innerHTML =
                `
                <p>
                    Encontramos
                    <strong>${receitas.length}</strong>
                    receita(s) para
                    <strong>${escapeHtml(termo)}</strong>.
                </p>
                `;


            receitasEncontradas.innerHTML =
                receitas
                    .map(criarCardReceita)
                    .join("");


            secaoResultados.style.display =
                "block";


            secaoResultados.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });

        }
        catch (erro) {

            console.error(
                "Erro ao buscar receitas:",
                erro
            );


            resultadoBusca.innerHTML =
                `
                <p>
                    Não foi possível realizar a busca.
                </p>
                `;

        }

    }


    function criarCardReceita(receita) {

        return `
            <article class="recipe-card">

                <img
                    src="${escapeHtml(receita.imagemUrl)}"
                    alt="${escapeHtml(receita.nome)}"
                />

                <div class="recipe-info">

                    <span class="country">
                        ${escapeHtml(receita.bandeira)}
                        ${escapeHtml(receita.pais)}
                    </span>

                    <h3>
                        ${escapeHtml(receita.nome)}
                    </h3>

                    <p>
                        ${escapeHtml(receita.descricao)}
                    </p>

                    <div class="recipe-details">

                        <span>
                            ⏱ ${receita.tempoPreparoMinutos} min
                        </span>

                        <span>
                            👨‍🍳 ${escapeHtml(receita.dificuldade)}
                        </span>

                    </div>

                </div>

            </article>
        `;

    }


    // =====================================================
    // CHEFE IA - ELEMENTOS
    // =====================================================

    const ingredienteIA =
        document.getElementById("ingredienteIA");

    const btnAdicionarIngrediente =
        document.getElementById(
            "btnAdicionarIngrediente"
        );

    const listaIngredientes =
        document.getElementById(
            "listaIngredientes"
        );

    const btnDescobrirIA =
        document.getElementById(
            "btnDescobrirIA"
        );

    const preferenciaIA =
        document.getElementById(
            "preferenciaIA"
        );

    const porcoesIA =
        document.getElementById(
            "porcoesIA"
        );

    const statusIA =
        document.getElementById(
            "statusIA"
        );

    const resultadoIA =
        document.getElementById(
            "resultadoIA"
        );


    const ingredientesSelecionados =
        [];


    // =====================================================
    // ADICIONAR INGREDIENTE
    // =====================================================

    if (
        btnAdicionarIngrediente &&
        ingredienteIA
    ) {

        btnAdicionarIngrediente
            .addEventListener(
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

        const ingrediente =
            ingredienteIA.value.trim();


        if (!ingrediente) {

            statusIA.innerHTML =
                mensagemAviso(
                    "Digite um ingrediente."
                );

            return;

        }


        const jaExiste =
            ingredientesSelecionados.some(
                item =>
                    item.toLowerCase() ===
                    ingrediente.toLowerCase()
            );


        if (jaExiste) {

            statusIA.innerHTML =
                mensagemAviso(
                    "Esse ingrediente já foi adicionado."
                );

            return;

        }


        ingredientesSelecionados.push(
            ingrediente
        );


        ingredienteIA.value =
            "";


        ingredienteIA.focus();


        statusIA.innerHTML =
            "";


        atualizarListaIngredientes();

    }


    // =====================================================
    // LISTA DE INGREDIENTES
    // =====================================================

    function atualizarListaIngredientes() {

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


                        ingredientesSelecionados
                            .splice(
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
    // BOTÃO CHEFE IA
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

            statusIA.innerHTML =
                mensagemAviso(
                    "Adicione pelo menos um ingrediente."
                );

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


        statusIA.innerHTML =
            `
            <div style="
                padding:12px;
                color:#76573d;
                font-size:13px;
            ">
                ✨ O Chefe IA está preparando
                sua receita...
            </div>
            `;


        btnDescobrirIA.disabled =
            true;


        const textoOriginalBotao =
            btnDescobrirIA.innerHTML;


        btnDescobrirIA.innerHTML =
            "👨‍🍳 Preparando...";


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

                statusIA.innerHTML =
                    mensagemLogin();


                setTimeout(
                    () => {

                        const returnUrl =
                            encodeURIComponent(
                                window.location
                                    .pathname +
                                window.location
                                    .search
                            );


                        window.location.href =
                            `/Conta/Login?returnUrl=${returnUrl}`;

                    },
                    900
                );


                return;

            }


            // =================================================
            // ACESSO NEGADO
            // =================================================

            if (
                response.status === 403
            ) {

                const dados =
                    await tentarLerJson(
                        response
                    );


                statusIA.innerHTML =
                    mensagemErro(
                        dados?.message ||
                        "Sua conta não possui permissão para utilizar a IA."
                    );


                return;

            }


            // =================================================
            // LIMITE MENSAL
            // =================================================

            if (
                response.status === 429
            ) {

                const dados =
                    await tentarLerJson(
                        response
                    );


                mostrarLimiteAtingido(
                    dados
                );


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
            // SUCESSO
            // =================================================

            const dados =
                await response.json();


            if (
                !dados ||
                dados.success !== true ||
                !dados.recipe
            ) {

                throw new Error(
                    "A resposta do servidor não contém uma receita válida."
                );

            }


            mostrarReceitaIA(
                dados.recipe
            );


            mostrarUsoPlano(
                dados.usage
            );

        }
        catch (erro) {

            console.error(
                "Erro ao consultar Chefe IA:",
                erro
            );


            statusIA.innerHTML =
                mensagemErro(
                    "Não foi possível obter uma sugestão agora."
                );

        }
        finally {

            btnDescobrirIA.disabled =
                false;


            btnDescobrirIA.innerHTML =
                textoOriginalBotao;

        }

    }


    // =====================================================
    // MOSTRAR USO DO PLANO
    // =====================================================

    function mostrarUsoPlano(usage) {

        if (!usage) {

            statusIA.innerHTML =
                "";

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
                <strong>${escapeHtml(nomePlano)}</strong>

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
    // LIMITE ATINGIDO
    // =====================================================

    function mostrarLimiteAtingido(dados) {

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
                    <strong>${usadas}/${limite}</strong>
                    consultas do seu plano Premium neste mês.
                  `
                : `
                    Você utilizou
                    <strong>${usadas}/${limite}</strong>
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
                            href="/"
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
    // MOSTRAR RECEITA DA IA
    // =====================================================

    function mostrarReceitaIA(receita) {

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
                `🌎 ${receita.pais || ""}`;

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
                `🍽 ${receita.categoria || ""}`;

        }


        if (tempo) {

            tempo.textContent =
                `⏱ ${receita.tempoMinutos || 0} min`;

        }


        if (porcoes) {

            porcoes.textContent =
                `👥 ${receita.porcoes || 1} porções`;

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

    function mensagemAviso(texto) {

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


    function mensagemErro(texto) {

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
    // TENTAR LER JSON
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
    // SEGURANÇA DE HTML
    // =====================================================

    function escapeHtml(valor) {

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
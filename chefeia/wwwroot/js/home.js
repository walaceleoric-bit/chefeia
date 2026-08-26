document.addEventListener("DOMContentLoaded", () => {

    // =====================================================
    // BUSCA DE RECEITAS
    // =====================================================

    const input = document.getElementById("ingredienteBusca");
    const botao = document.getElementById("btnBuscar");

    const resultadoBusca =
        document.getElementById("resultadoBusca");

    const secaoResultados =
        document.getElementById("secaoResultados");

    const receitasEncontradas =
        document.getElementById("receitasEncontradas");


    if (botao && input) {

        botao.addEventListener("click", buscarReceitas);


        input.addEventListener("keydown", event => {

            if (event.key === "Enter") {

                event.preventDefault();

                buscarReceitas();

            }

        });

    }


    async function buscarReceitas() {

        const termo = input.value.trim();


        if (!termo) {

            resultadoBusca.innerHTML =
                "<p>Digite algo para pesquisar.</p>";

            return;
        }


        resultadoBusca.innerHTML =
            "<p>🔎 Buscando receitas...</p>";


        secaoResultados.style.display = "none";

        receitasEncontradas.innerHTML = "";


        try {

            const response = await fetch(
                `/api/receitas/buscar?termo=${encodeURIComponent(termo)}`
            );


            if (!response.ok) {

                throw new Error(
                    "Erro ao consultar receitas."
                );

            }


            const receitas = await response.json();


            if (receitas.length === 0) {

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


            secaoResultados.style.display = "block";


            secaoResultados.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });

        }
        catch (erro) {

            console.error(erro);

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
    // CHEFE IA - INGREDIENTES
    // =====================================================

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


    const ingredientesSelecionados = [];


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

        const ingrediente =
            ingredienteIA.value.trim();


        if (!ingrediente) {

            statusIA.innerHTML =
                "<p>Digite um ingrediente.</p>";

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
                "<p>Esse ingrediente já foi adicionado.</p>";

            return;

        }


        ingredientesSelecionados.push(
            ingrediente
        );


        ingredienteIA.value = "";

        ingredienteIA.focus();

        statusIA.innerHTML = "";

        atualizarListaIngredientes();

    }


    function atualizarListaIngredientes() {

        listaIngredientes.innerHTML =
            ingredientesSelecionados
                .map((ingrediente, indice) => {

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

                })
                .join("");


        const botoesRemover =
            document.querySelectorAll(
                ".remover-ingrediente"
            );


        botoesRemover.forEach(botaoRemover => {

            botaoRemover.addEventListener(
                "click",
                () => {

                    const indice =
                        Number(
                            botaoRemover.dataset.indice
                        );


                    ingredientesSelecionados.splice(
                        indice,
                        1
                    );


                    atualizarListaIngredientes();

                }
            );

        });

    }


    // =====================================================
    // CHAMADA DO ENDPOINT DA IA
    // =====================================================

    if (btnDescobrirIA) {

        btnDescobrirIA.addEventListener(
            "click",
            consultarChefeIA
        );

    }


    async function consultarChefeIA() {

        if (
            ingredientesSelecionados.length === 0
        ) {

            statusIA.innerHTML =
                "<p>Adicione pelo menos um ingrediente.</p>";

            return;

        }


        const porcoes =
            Number(porcoesIA.value);


        const consulta = {

            ingredientes:
                ingredientesSelecionados,

            preferencia:
                preferenciaIA.value,

            porcoes:
                porcoes > 0
                    ? porcoes
                    : 1
        };


        statusIA.innerHTML =
            "<p>✨ O Chefe IA está preparando uma sugestão...</p>";


        btnDescobrirIA.disabled = true;


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
                            JSON.stringify(consulta)
                    }
                );


            if (!response.ok) {

                throw new Error(
                    `Erro HTTP: ${response.status}`
                );

            }


            const receita =
                await response.json();


            mostrarReceitaIA(receita);


            statusIA.innerHTML = "";

        }
        catch (erro) {

            console.error(
                "Erro ao consultar Chefe IA:",
                erro
            );


            statusIA.innerHTML =
                `
                <p>
                    Não foi possível obter
                    uma sugestão agora.
                </p>
                `;

        }
        finally {

            btnDescobrirIA.disabled = false;

        }

    }


    // =====================================================
    // MOSTRAR RECEITA DA IA
    // =====================================================

    function mostrarReceitaIA(receita) {

        document.getElementById(
            "paisReceitaIA"
        ).textContent =
            `🌎 ${receita.pais || ""}`;


        document.getElementById(
            "nomeReceitaIA"
        ).textContent =
            receita.nome || "Receita sugerida";


        document.getElementById(
            "descricaoReceitaIA"
        ).textContent =
            receita.descricao || "";


        document.getElementById(
            "categoriaReceitaIA"
        ).textContent =
            `🍽 ${receita.categoria || ""}`;


        document.getElementById(
            "tempoReceitaIA"
        ).textContent =
            `⏱ ${receita.tempoMinutos || 0} min`;


        document.getElementById(
            "porcoesReceitaIA"
        ).textContent =
            `👥 ${receita.porcoes || 1} porções`;


        const listaIngredientesReceita =
            document.getElementById(
                "ingredientesReceitaIA"
            );


        listaIngredientesReceita.innerHTML =
            (receita.ingredientes || [])
                .map(
                    ingrediente =>
                        `<li>${escapeHtml(ingrediente)}</li>`
                )
                .join("");


        const listaPassos =
            document.getElementById(
                "passosReceitaIA"
            );


        listaPassos.innerHTML =
            (receita.passos || [])
                .map(
                    passo =>
                        `<li>${escapeHtml(passo)}</li>`
                )
                .join("");


        resultadoIA.style.display =
            "block";


        resultadoIA.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });

    }


    // =====================================================
    // SEGURANÇA PARA TEXTO INSERIDO NO HTML
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
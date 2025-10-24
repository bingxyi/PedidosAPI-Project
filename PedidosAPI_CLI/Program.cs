using System;
using System.Linq;
using PedidosAPI.Data;
using PedidosAPI.Models;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Iniciando CLI ...");

// 1. Encontrando o caminho para o arquivo do banco de dados
// Sai da pasta (bin/Debug/net9.0) do CLI para a raiz 'DB' e entra na pasta da API
var dbPath = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, // Onde o .exe está (PedidosAPI_CLI/bin/Debug/net9.0)
    "..",                     // -> PedidosAPI_CLI/bin/Debug
    "..",                     // -> PedidosAPI_CLI/bin
    "..",                     // -> PedidosAPI_CLI
    "..",                     // -> DB
    "PedidosAPI",             // -> DB/PedidosAPI
    "pedidos.db"              // -> DB/PedidosAPI/pedidos.db
));
var connectionString = $"Data Source={dbPath}";

// 2. Criando as opções do DbContext manualmente
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlite(connectionString);
var dbOptions = optionsBuilder.Options;

bool sair = false;
while (!sair)
{
    Console.WriteLine("\n--- PedidosAPI CLI ---");
    Console.WriteLine("1. [GET] Listar todos os pedidos");
    Console.WriteLine("2. [GET {id}] Buscar pedido por ID");
    Console.WriteLine("3. [POST] Adicionar novo pedido");
    Console.WriteLine("4. [DELETE {id}] Deletar pedido");
    Console.WriteLine("5. [PUT] Corrigir Totais no Banco");
    Console.WriteLine("0. Sair");
    Console.Write("Escolha uma opção: ");

    string? escolha = Console.ReadLine();
    
    // Usamos 'await' para esperar as operações de banco
    switch (escolha)
    {
        case "1":
            await ListarTodosPedidos(dbOptions);
            break;
        case "2":
            await BuscarPedidoPorId(dbOptions);
            break;
        case "3":
            await AdicionarNovoPedido(dbOptions);
            break;
        case "4":
            await DeletarPedido(dbOptions);
            break;
        case "5":
            await CorrigirTotais(dbOptions);
            break;
        case "0":
            sair = true;
            break;
        default:
            Console.WriteLine("Opção inválida.");
            break;
    }
}

// Funções CRUD

// 1. [GET] Listar todos
async Task ListarTodosPedidos(DbContextOptions<AppDbContext> options)
{
    // Criamos um novo contexto para cada operação
    using (var db = new AppDbContext(options))
    {
        Console.WriteLine("\n--- Listando Pedidos ---");
        var pedidos = await db.Pedidos
            .Include(p => p.Itens)
            .AsNoTracking()
            .ToListAsync();

        if (!pedidos.Any())
        {
            Console.WriteLine("Nenhum pedido encontrado.");
            return;
        }

        foreach (var pedido in pedidos)
        {
            // Calculamos o total na hora, pois o do banco está errado
            decimal totalCalculado = pedido.Itens.Sum(i => i.Subtotal);

            Console.WriteLine($"[Pedido ID: {pedido.Id}] Cliente: {pedido.Cliente}");
            Console.WriteLine($"  Total (Salvo no DB): {pedido.Total:C}"); // O valor errado (R$ 0,00)
            Console.WriteLine($"  Total (Calculado):   {totalCalculado:C}"); // O valor correto

            foreach (var item in pedido.Itens)
            {
                // Agora mostrando o preço unitário
                Console.WriteLine($"  -> Item: {item.Produto}, Qtd: {item.Quantidade}, Preço Unit: {item.PrecoUnitario:C}");
            }
            Console.WriteLine("-------------------------");
        }
    }
}

// 2. [GET {id}] Buscar por ID
async Task BuscarPedidoPorId(DbContextOptions<AppDbContext> options)
{
    using (var db = new AppDbContext(options))
    {
        Console.Write("Digite o ID do pedido: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido.");
            return;
        }

        var pedido = await db.Pedidos
            .Include(p => p.Itens)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            Console.WriteLine($"Pedido com ID {id} não encontrado.");
            return;
        }

        decimal totalCalculado = pedido.Itens.Sum(i => i.Subtotal);
        Console.WriteLine($"\n--- Pedido ID: {pedido.Id} ---");
        Console.WriteLine($"Cliente: {pedido.Cliente}");
        Console.WriteLine($"Data: {pedido.Data}");
        Console.WriteLine($"Total (Salvo): {pedido.Total:C} / Total (Calculado): {totalCalculado:C}");
        Console.WriteLine("Itens:");
        foreach (var item in pedido.Itens)
        {
            Console.WriteLine($"  -> {item.Produto} (Qtd: {item.Quantidade}, Preço Unit: {item.PrecoUnitario:C})");
        }
    }
}

// 3. [POST] Adicionar novo
async Task AdicionarNovoPedido(DbContextOptions<AppDbContext> options)
{
    using (var db = new AppDbContext(options))
    {
        Console.WriteLine("\n--- Adicionar Novo Pedido ---");
        Console.Write("Nome do Cliente: ");
        string? cliente = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(cliente))
        {
            Console.WriteLine("Nome do cliente não pode ser vazio.");
            return;
        }

        var novoPedido = new Pedido
        {
            Cliente = cliente,
            Data = DateTime.Now,
            Itens = new List<Item>()
        };

        // Loop para adicionar itens
        while (true)
        {
            Console.Write("Nome do Produto (ou 's' para sair): ");
            string? produto = Console.ReadLine();
            if (produto?.ToLower() == "s" || string.IsNullOrWhiteSpace(produto))
                break;

            Console.Write("Quantidade: ");
            if (!int.TryParse(Console.ReadLine(), out int qtd) || qtd <= 0)
            {
                Console.WriteLine("Quantidade inválida.");
                continue;
            }

            Console.Write("Preço Unitário: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal preco) || preco <= 0)
            {
                Console.WriteLine("Preço inválido.");
                continue;
            }
            
            novoPedido.Itens.Add(new Item
            {
                Produto = produto,
                Quantidade = qtd,
                PrecoUnitario = preco,
                Pedido = novoPedido // Vincula o item ao pedido
            });
            Console.WriteLine("Item adicionado.");
        }

        if (!novoPedido.Itens.Any())
        {
            Console.WriteLine("Pedido cancelado (nenhum item).");
            return;
        }

        // !! IMPORTANTE: Calculando o total antes de salvar !!
        novoPedido.Total = novoPedido.Itens.Sum(i => i.Subtotal);

        db.Pedidos.Add(novoPedido);
        await db.SaveChangesAsync();

        Console.WriteLine($"Pedido para '{novoPedido.Cliente}' (Total: {novoPedido.Total:C}) salvo com ID: {novoPedido.Id}");
    }
}

// 4. [DELETE {id}] Deletar
async Task DeletarPedido(DbContextOptions<AppDbContext> options)
{
    using (var db = new AppDbContext(options))
    {
        Console.Write("Digite o ID do pedido a ser DELETADO: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido.");
            return;
        }
        
        // Usamos FindAsync pois só precisamos do ID para deletar
        var pedido = await db.Pedidos.FindAsync(id); 
        
        if (pedido == null)
        {
            Console.WriteLine($"Pedido com ID {id} não encontrado.");
            return;
        }

        Console.Write($"Tem certeza que deseja deletar o pedido de '{pedido.Cliente}' (ID: {id})? (s/n): ");
        if (Console.ReadLine()?.ToLower() != "s")
        {
            Console.WriteLine("Operação cancelada.");
            return;
        }

        db.Pedidos.Remove(pedido);
        await db.SaveChangesAsync(); // O Cascade fará o resto (deletar os itens)
        
        Console.WriteLine("Pedido deletado com sucesso.");
    }
}

// 5. [PUT] Corrigir Totais (Bônus)
async Task CorrigirTotais(DbContextOptions<AppDbContext> options)
{
    using (var db = new AppDbContext(options))
    {
        Console.WriteLine("\n--- Corrigindo Totais no Banco ---");
        // Pega todos os pedidos que têm total 0, mas que têm itens
        var pedidosParaCorrigir = await db.Pedidos
            .Include(p => p.Itens)
            .Where(p => p.Total == 0 && p.Itens.Any())
            .ToListAsync();
        
        if (!pedidosParaCorrigir.Any())
        {
            Console.WriteLine("Nenhum pedido com total zerado encontrado.");
            return;
        }

        int contador = 0;
        foreach (var pedido in pedidosParaCorrigir)
        {
            decimal totalCorreto = pedido.Itens.Sum(i => i.Subtotal);
            Console.WriteLine($"Corrigindo Pedido ID {pedido.Id} ('{pedido.Cliente}')... Total antigo: R$ 0,00 -> Novo Total: {totalCorreto:C}");
            pedido.Total = totalCorreto;
            contador++;
        }

        await db.SaveChangesAsync();
        Console.WriteLine($"\n{contador} pedidos foram corrigidos no banco.");
    }
}
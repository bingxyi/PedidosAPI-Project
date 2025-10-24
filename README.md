# 🛍️ Projeto API de Pedidos (+ CLI)
> Um sistema completo em C# e .NET 9 para gerenciamento de pedidos, construído com uma arquitetura limpa que inclui uma API RESTful (para consumo web) e uma aplicação de console (CLI) para gerenciamento direto.Ambas as aplicações compartilham o mesmo banco de dados SQLite e a mesma lógica de negócios (Models e DbContext) via Entity Framework Core.<p align="center"><img src="https://img.shields.io/badge/C%23-11-blueviolet?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C# 11"><img src="https://img.shields.io/badge/.NET-9-blue?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9"><img src="https://img.shields.io/badge/ASP.NET-Core-blueviolet?style=for-the-badge&logo=aspnet&logoColor=white" alt="ASP.NET Core"><img src="https://img.shields.io/badge/Entity%20Framework-Core-blueviolet?style=for-the-badge" alt="EF Core"><img src="https://img.shields.io/badge/SQLite-blue?style=for-the-badge&logo=sqlite&logoColor=white" alt="SQLite"></p>
## ✨ Funcionalidades
- API RESTful Completa: Operações CRUD (Create, Read, Update, Delete) para Pedidos e Itens.
- Aplicação de Console (CLI): Uma interface de linha de comando robusta para gerenciar o banco de dados diretamente, ideal para testes rápidos ou scripts.
- Banco de Dados Único: A API e o CLI acessam e manipulam o mesmo arquivo de banco de dados pedidos.db.
- Migrations com EF Core: A estrutura do banco é gerenciada por "migrations", facilitando a criação e atualização do schema.
- Relacionamento 1-N: Um Pedido pode ter múltiplos Itens, configurado com deleção em cascata (ao deletar um pedido, seus itens são deletados).

# 🚀 Como Começar (Passos para Rodar)
> Siga estes passos para configurar e executar o projeto localmente.
### Pré-requisitos.
- NET 9 SDK (ou a versão utilizada no projeto).
- (Recomendado) A ferramenta de linha de comando do EF Core. Instale globalmente com:
```bash
dotnet tool install --global dotnet-ef 
```
- Um cliente de API como Postman, Thunder Client (extensão do VS Code) ou use o arquivo .http incluído. 

#### Passo 1: Clonar e Preparar o Banco
> O arquivo do banco (pedidos.db) não está no repositório (por boas práticas, ele está no .gitignore). Você precisa criá-lo a partir das Migrations.
```bash
# 1. Clone o repositório
git clone https://github.com/SEU-USUARIO/SEU-REPOSITORIO.git
cd SEU-REPOSITORIO

# 2. Entre na pasta da API
cd PedidosAPI

# 3. Execute o comando para aplicar as migrations
dotnet ef database update
```

#### Passo 2: Executar a APIA API precisa estar rodando para receber requisições HTTP.
```bash
# 1. (Se não estiver) Entre na pasta da API
cd PedidosAPI

# 2. Execute o projeto
dotnet run
```

O terminal mostrará as URLs onde a API está escutando, similar a:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7123
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5123
```

#### Passo 3: Executar o CLI (Opcional)
> Você pode usar o CLI para interagir com o banco ao mesmo tempo que a API.
```bash
# 1. Abra um NOVO terminal

# 2. Navegue até a pasta do CLI
cd PedidosAPI_CLI

# 3. Execute o projeto
dotnet run
```

O menu do CLI aparecerá, permitindo listar, adicionar ou deletar pedidos.

# 🗂️ Estrutura das Entidades (Models)O banco de dados é composto por duas tabelas principais, `Pedidos` e `Itens`. 

`Pedido.cs`
> Representa o pedido de um cliente.
```C#
public class Pedido
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Cliente { get; set; }

    [Required]
    public DateTime Data { get; set; } = DateTime.Now;

    [Range(0, double.MaxValue)]
    public decimal Total { get; set; } = 0;

    // Relacionamento: 1 Pedido tem N Itens
    public List<Item> Itens { get; set; } = new();
}
```

`Item.cs`
> Representa um item dentro de um pedido.
```C#
public class Item
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Produto { get; set; }

    [Range(1, 9999)]
    public int Quantidade { get; set; }

    [Range(0.01, 100000)]
    public decimal PrecoUnitario { get; set; }

    [NotMapped]
    public decimal Subtotal => Quantidade * PrecoUnitario;

    // Chave Estrangeira para o Pedido
    public int PedidoId { get; set; }

    [JsonIgnore] // Ignora a propriedade na serialização JSON
    public Pedido? Pedido { get; set; }
}
```

# 🗺️ Rotas da API (Endpoints)
Base URL: `http://localhost:PORT/api` (PORT = porta que o terminal indicar, ex.: 5001)

`PedidosController (/api/pedidos)`

Método | Rota | Descrição
GET | /api/pedidos | "Retorna uma lista de todos os pedidos, incluindo seus itens."
GET | /api/pedidos/{id} | "Retorna um pedido específico pelo seu Id, incluindo seus itens."
POST | /api/pedidos | Cria um novo pedido. O total é calculado automaticamente.
PUT | /api/pedidos/{id} | Atualiza um pedido existente. (Requer o corpo completo do pedido).
DELETE | /api/pedidos/{id} | Deleta um pedido e todos os seus itens (via Cascade Delete).

`ItensController (/api/itens)`

Método | Rota | Descrição
GET | /api/itens | Retorna uma lista de todos os itens de todos os pedidos.
GET | /api/itens/{id} | Retorna um item específico pelo seu Id.
PUT | /api/itens/{id} | Atualiza um item específico.
DELETE | /api/itens/{id} | Deleta um item específico do banco.

# 🧪 Como Testar a Aplicação

#### 1. Testando com a Aplicação CLI

O CLI é a forma mais direta de verificar se os dados estão sendo salvos.

1. Vá para a pasta PedidosAPI_CLI.

2. Execute dotnet run.

3. Use o menu:

```
--- PedidosAPI CLI ---
1. [GET] Listar todos os pedidos
2. [GET {id}] Buscar pedido por ID
3. [POST] Adicionar novo pedido
4. [DELETE {id}] Deletar pedido
5. [PUT] Corrigir Totais no Banco
0. Sair
```

#### 2. Testando com Arquivo .http (VS Code)
Se você usa o Visual Studio Code, pode usar a extensão REST Client e o arquivo `PedidosAPI.http` (que já deve estar no seu projeto).
Basta abri-lo e clicar em Send Request em cima da requisição que deseja fazer.

Exemplo (`PedidosAPI.http`):

```HTTP
@baseUrl = https://localhost:PORT/api

### [GET] Listar todos os pedidos
GET {{baseUrl}}/pedidos

### [GET] Buscar pedido por ID
GET {{baseUrl}}/pedidos/1

### [POST] Criar um novo pedido
POST {{baseUrl}}/pedidos
Content-Type: application/json

{
    "cliente": "Cliente de Teste (HTTP)",
    "itens": [
        {
            "produto": "Teclado Mecânico",
            "quantidade": 1,
            "precoUnitario": 350.0
        },
        {
            "produto": "Mouse Gamer",
            "quantidade": 2,
            "precoUnitario": 150.0
        }
    ]
}

### [DELETE] Deletar um pedido
DELETE {{baseUrl}}/pedidos/3
```

#### 3. Testando com Postman / Thunder Client

- Certifique-se que a API (PedidosAPI) esteja rodando (dotnet run).
- Crie uma nova requisição no seu cliente de API.

Exemplo: Criando um Pedido [POST]
1. Método: POST
2. URL: https://localhost:7123/api/pedidos
3. Selecione a aba Body (Corpo).
4. Escolha o tipo raw.
5. Escolha o formato JSON.
6. Cole o JSON de exemplo abaixo:
<details> <summary>Clique para ver o Exemplo de JSON (POST Pedido)</summary>

```JSON
{
    "cliente": "Ana Souza (via Postman)",
    "data": "2025-10-24T17:00:00",
    "itens": [
        {
            "produto": "Teclado Mecânico Razer",
            "quantidade": 1,
            "precoUnitario": 320.0
        },
        {
            "produto": "Mouse Gamer Corsair",
            "quantidade": 2,
            "precoUnitario": 150.0
        }
    ]
}
```
</details>

Exemplo: Atualizando um Item [PUT]
1. Método: PUT
2. URL: https://localhost:7123/api/itens/1 (para atualizar o item de ID 1)
3. Selecione a aba Body (Corpo) -> raw -> JSON.
4. Cole o JSON de exemplo abaixo:
<details> <summary>Clique para ver o Exemplo de JSON (PUT Item)</summary>
  
```JSON 
{
    "id": 1,
    "produto": "Teclado Mecânico (ATUALIZADO)",
    "quantidade": 1,
    "precoUnitario": 325.50,
    "pedidoId": 1
}
```
</details>


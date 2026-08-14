# GEMINI.md — Instruções do Projeto Up4All.MessageBus

Este arquivo contém as diretrizes arquiteturais, padrões de projeto e fluxos de trabalho recomendados para desenvolvimento na solução **Up4All.MessageBus** utilizando o Gemini CLI.

---

## 🏗️ Estrutura de Pastas e Projetos

A solução está organizada de forma modular, dividindo as abstrações essenciais das implementações de infraestrutura específicas de cada message broker:

- `src/Up4All.Framework.MessageBus.Abstractions/`
  - Contém todas as interfaces (`IMessage`, `IMessageBusPublisherAsync`, `IMessageBusAsyncConsumer`), modelos base (`MessageBusMessage`, `ReceivedMessage`), enums, atributos, opções globais e a base do pipeline de interceptação.
  - Alvo: `netstandard2.1` para máxima portabilidade em ecossistemas .NET Framework / Core.

- `src/Up4All.Framework.MessageBus.RabbitMQ/`
  - Implementação de clientes, publishers, consumidores e pipelines específicos para o **RabbitMQ** (utilizando a biblioteca oficial `RabbitMQ.Client`).

- `src/Up4All.Framework.MessageBus.ServiceBus/`
  - Implementação para o **Azure Service Bus** utilizando o SDK oficial `Azure.Messaging.ServiceBus`.

- `src/Up4All.Framework.MessageBus.Kafka/`
  - Implementação para o **Apache Kafka** utilizando o cliente `Confluent.Kafka`.

- `src/Up4All.Framework.MessageBus.TransferHelper/`
  - Hosted Service e utilitários especializados para Shovel/Forward, permitindo transferir e transformar mensagens dinamicamente entre diferentes brokers (ex: consumir do Azure Service Bus e publicar no RabbitMQ).

- `tests/Up4All.Framework.MessageBus.Mocks/`
  - Biblioteca de Mocks reusáveis dos clientes assíncronos e síncronos do Message Bus para simplificar testes de unidade de aplicações consumidoras.

- `Up4All.Framework.MessageBus.Tests/`
  - Projeto principal de testes unitários e de integração utilizando **NUnit 4** sob o target `net8.0`.

---

## 🎯 Padrões de Projeto e Práticas Obrigatórias

Ao codificar ou sugerir alterações nesta solução, siga rigorosamente os seguintes padrões:

### 1. Injeção de Dependência Desacoplada
- Forneça sempre extensões fluídas de registro de dependências no `IServiceCollection` dentro de classes chamadas `IoCExtensions.cs` no namespace `<Projeto>.Extensions`.
- Evite que as aplicações clientes precisem referenciar diretamente tipos internos do broker. As injeções devem expor interfaces como `IMessageBusQueueAsyncClient` ou `IMessageBusStandalonePublisherAsync`.

### 2. Programação Assíncrona Baseada em Tasks
- Métodos de comunicação com brokers devem ser estritamente assíncronos.
- Sempre propague o `CancellationToken` em todas as assinaturas de métodos e repasse-o para as APIs de baixo nível (como RabbitMQ ou Azure Service Bus SDKs).
- Métodos assíncronos devem ter o sufixo `Async` de forma consistente.

### 3. Construtores Primários (C# 12)
- Como especificado em `Directory.Build.props`, utilizamos a versão mais recente do compilador C#.
- Dê preferência absoluta para o uso de **Primary Constructors** ao injetar serviços ou opções nas classes em novos desenvolvimentos ou refatorações:
  ```csharp
  public class MeuServico(ILogger<MeuServico> logger, IOptions<MinhasOpcoes> options)
  {
      private readonly ILogger<MeuServico> _logger = logger;
      private readonly MinhasOpcoes _options = options.Value;
  }
  ```

### 4. Observabilidade e Tracing Integrado
- O MessageBus implementa propagação de tracing distribuído via **OpenTelemetry** usando `Activity` do .NET.
- Mensagens enviadas devem injetar o contexto de trace nos cabeçalhos (`UserProperties`), e mensagens recebidas devem extrair esse contexto para manter a correlação entre microsserviços. Use os helpers disponíveis em `OpenTelemetryExtensions.cs` no projeto de Abstractions.

### 5. Resiliência de Conexão
- Lógicas de reconexão automática e tolerância a falhas transitórias devem ser implementadas usando **Polly** nos clientes de infraestrutura.
- Respeite o parâmetro `ConnectionAttempts` das opções de configuração globais.

---

## 🧪 Estratégia de Testes

- Qualquer nova funcionalidade adicionada nos provedores ou nas abstrações deve ser acompanhada por testes unitários no projeto `Up4All.Framework.MessageBus.Tests`.
- Utilize os Mocks de `Up4All.Framework.MessageBus.Mocks` ou crie testes unitários usando frameworks de mock tradicionais (se estabelecido no projeto) para testar os pipelines, handlers e comportamentos de transferência.
- Comandos para rodar testes:
  ```bash
  dotnet test Up4All.MessageBus.sln
  ```

# AGENTS.md — Configuração e Diretrizes para Agentes de IA (Up4All.MessageBus)

Este arquivo descreve as personalidades, papéis, subagentes especializados e fluxos de trabalho que as IAs (como o Gemini CLI) devem adotar ao trabalhar na solução **Up4All.MessageBus**.

---

## 🤖 Papéis dos Subagentes Recomendados

Para acelerar o desenvolvimento de novas funcionalidades e correções, as tarefas devem ser divididas de acordo com os seguintes perfis especializados:

### 1. `codebase_investigator` (Investigador do Codebase)
- **Foco principal**: Diagnosticar problemas de concorrência, vazamentos de memória, falhas na reconexão de canais (ex: RabbitMQ Channel/Connection) e análise de ciclo de vida do Azure Service Bus Processor.
- **Tarefas típicas**:
  - Investigar por que um consumidor do RabbitMQ para de processar mensagens repentinamente após oscilações de rede.
  - Mapear a propagação de tracing distribuído do OpenTelemetry em todos os drivers para garantir que as propriedades de diagnóstico não sejam perdidas durante transferências de broker.
  - Avaliar o descarte (`Dispose/DisposeAsync`) correto de conexões de clientes síncronos e assíncronos.

### 2. `generalist` (Engenheiro de Refatoração e Geração em Massa)
- **Foco principal**: Executar refatorações repetitivas em larga escala, padronizar construtores em massa e preencher lacunas de testes unitários.
- **Tarefas típicas**:
  - Migrar todas as classes utilitárias e construtores tradicionais para o padrão de **Primary Constructors (C# 12)**.
  - Implementar suporte a novos frameworks nas dependências de NuGet (ex: atualizar pacotes de extensões de injeção de dependência Microsoft.Extensions para a versão estável mais recente).
  - Gerar arquivos de infraestrutura adicionais (ex: novos testes de unidade para simulação de recebimento em Mock).

### 3. `cli_help` (Auxiliar de Integração de Ferramentas)
- **Foco principal**: Configuração de ambientes locais, scripts de execução automatizada e auxílio no uso do CLI local.
- **Tarefas típicas**:
  - Validar e sugerir scripts PowerShell/Bash para subir ambientes Docker locais com RabbitMQ, Kafka e LocalStack para simular brokers.

---

## ⚙️ Diretrizes Operacionais para os Agentes

Qualquer agente operando nesta solução deve obedecer às seguintes regras de ouro:

1. **Prioridade de Sobrecarga Assíncrona**: O projeto é altamente voltado para processamento não bloqueante e assíncrono. Nunca implemente métodos síncronos se as assinaturas assíncronas correspondentes existirem ou puderem ser expostas.
2. **Preservação de Compatibilidade Multitarget**: As abstrações devem rodar em `.NET Standard 2.1`. Evite utilizar APIs do .NET 8 ou superior nas Abstractions a menos que estejam protegidas por diretivas de compilação condicional (`#if NET8_0_OR_GREATER`).
3. **Nomenclatura Limpa de Extensões**: Os métodos de IoC devem ser intuitivos (ex: `AddMessageBusQueueClient<TClient>`). Mantenha a assinatura limpa, lendo as conexões a partir de `IConfiguration` de maneira amigável.
4. **Verificação de Bugs Empírica**: Antes de declarar um bug como corrigido nos drivers de infraestrutura (RabbitMQ, Service Bus, Kafka), o agente deve escrever e executar um caso de teste que reproduza deterministicamente a falha e mostre o sucesso após o patch.

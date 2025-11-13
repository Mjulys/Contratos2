# 🚀 Guia Completo: Publicar Contratos2 no Azure

Este guia vai te levar do zero até ter sua aplicação rodando no Azure!

---

## 📋 Pré-requisitos

- ✅ Conta Microsoft (pode usar Outlook, Hotmail, etc.)
- ✅ Visual Studio 2022 instalado (ou VS Code)
- ✅ Projeto já commitado no GitHub (✅ já está!)

---

## PASSO 1: Criar Conta Azure Gratuita

1. Acesse: **https://azure.microsoft.com/free/**
2. Clique em **"Start free"** ou **"Iniciar gratuitamente"**
3. Faça login com sua conta Microsoft
4. Preencha os dados:
   - País: Portugal (ou seu país)
   - Telefone: seu número
   - Cartão de crédito: **NÃO será cobrado** (apenas validação)
5. Aceite os termos e clique em **"Sign up"**

**🎁 Você ganha:**
- $200 em créditos por 30 dias
- 12 meses de serviços gratuitos (incluindo SQL Database)
- Sempre grátis: App Service F1 tier

---

## PASSO 2: Criar SQL Database no Azure

### 2.1 Acessar o Portal Azure

1. Acesse: **https://portal.azure.com**
2. Faça login

### 2.2 Criar SQL Server

1. No topo, clique em **"Create a resource"** (Criar um recurso)
2. Na busca, digite: **"SQL Database"**
3. Clique em **"SQL Database"**
4. Clique no botão **"Create"** (Criar)

### 2.3 Configurar SQL Database

Preencha os campos:

**Basics (Básico):**
- **Subscription** (Assinatura): Escolha sua assinatura (Free Trial)
- **Resource Group**: 
  - Clique em **"Create new"**
  - Nome: `Contratos2-RG`
  - Clique em **OK**

**Database details:**
- **Database name**: `Contratos2DB`

**Server:**
- Clique em **"Create new"** ao lado de Server
  - **Server name**: `contratos2-server` (ou outro nome único)
  - **Location**: Escolha a região mais próxima (ex: "West Europe")
  - **Authentication method**: SQL authentication
  - **Server admin login**: `admincontratos` (ou outro username)
  - **Password**: Crie uma senha forte! **ANOTE ESTA SENHA!**
    - Exemplo: `MinhaSenh@Segura123!`
  - **Confirm password**: Digite novamente
  - Clique em **OK**

**Want to use SQL elastic pool?**
- Selecione: **No**

**Compute + storage:**
- Clique em **"Configure database"**
- Escolha: **Basic** (5 DTU) - **GRÁTIS por 12 meses!**
- Clique em **Apply**

**Backup storage:**
- Deixe o padrão (Locally-redundant backup storage)

5. Clique em **"Review + create"** (Revisar + criar)
6. Aguarde a validação
7. Clique em **"Create"** (Criar)

⏳ **Aguarde 2-3 minutos** enquanto o Azure cria o servidor e banco de dados.

---

## PASSO 3: Configurar Firewall do SQL Server

### 3.1 Permitir Acesso do Azure

1. Quando a criação terminar, clique em **"Go to resource"**
2. No menu lateral, vá em **"Security"** > **"Networking"**
3. Em **"Public network access"**, certifique-se que está **"Enabled"**
4. Em **"Firewall rules"**:
   - Clique em **"+ Add your client IPv4 address"** (adiciona seu IP)
   - Clique em **"+ Add 0.0.0.0 - 255.255.255.255"** (permite todos os IPs do Azure)
     - Nome: `AllowAzureServices`
     - Start IP: `0.0.0.0`
     - End IP: `0.0.0.0`
   - Clique em **"Add"**
5. Clique em **"Save"** no topo

### 3.2 Obter Connection String

1. No menu lateral, vá em **"Connection strings"**
2. Copie a connection string **ADO.NET**
3. **IMPORTANTE**: Substitua `{your_password}` pela senha que você criou
4. **GUARDE esta connection string!** Você vai precisar dela.

**Exemplo de connection string:**
```
Server=tcp:contratos2-server.database.windows.net,1433;Initial Catalog=Contratos2DB;Persist Security Info=False;User ID=admincontratos;Password=MinhaSenh@Segura123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

---

## PASSO 4: Criar App Service no Azure

### 4.1 Criar App Service

1. No portal Azure, clique em **"Create a resource"**
2. Na busca, digite: **"Web App"**
3. Clique em **"Web App"**
4. Clique em **"Create"**

### 4.2 Configurar App Service

**Basics:**
- **Subscription**: Escolha sua assinatura
- **Resource Group**: Selecione `Contratos2-RG` (o mesmo do SQL)
- **Name**: `contratos2-app` (ou outro nome único)
  - ⚠️ Este será o nome da URL: `https://contratos2-app.azurewebsites.net`
- **Publish**: **Code**
- **Runtime stack**: **.NET 8**
- **Operating System**: **Windows** (recomendado) ou Linux
- **Region**: Escolha a mesma região do SQL Database

**App Service Plan:**
- Clique em **"Create new"**
  - **Name**: `Contratos2-Plan`
  - **Operating System**: Windows (ou Linux)
  - **Region**: Mesma do App Service
  - **Pricing tier**: 
    - Clique em **"Dev/Test"**
    - Selecione **"F1" (FREE)** - Grátis para sempre!
    - Clique em **"Apply"**

**Deployment:**
- Deixe o padrão (sem CI/CD por enquanto)

**Monitoring:**
- **Application Insights**: **No** (para economizar)

5. Clique em **"Review + create"**
6. Clique em **"Create"**

⏳ **Aguarde 1-2 minutos** enquanto o Azure cria o App Service.

---

## PASSO 5: Configurar Connection String no App Service

1. Quando a criação terminar, clique em **"Go to resource"**
2. No menu lateral, vá em **"Configuration"**
3. Na aba **"Connection strings"**, clique em **"+ New connection string"**
4. Preencha:
   - **Name**: `DefaultConnection`
   - **Value**: Cole a connection string que você salvou (com a senha substituída)
   - **Type**: **SQLAzure** (ou SQL Server)
5. Clique em **"OK"**
6. **IMPORTANTE**: Clique em **"Save"** no topo da página
7. Clique em **"Continue"** quando perguntar sobre reiniciar

---

## PASSO 6: Publicar do Visual Studio

### 6.1 Preparar Publicação

1. Abra o projeto **Contratos2** no Visual Studio 2022
2. Clique com botão direito no projeto **Contratos2** no Solution Explorer
3. Selecione **"Publish"** (Publicar)

### 6.2 Conectar ao Azure

1. Na janela de publicação, escolha:
   - **Azure** > **Azure App Service (Windows)** ou **Azure App Service (Linux)**
2. Clique em **"Next"**

### 6.3 Selecionar App Service

1. Faça login no Azure (se necessário)
2. Selecione:
   - **Subscription**: Sua assinatura
   - **Resource Group**: `Contratos2-RG`
   - **App Service**: `contratos2-app` (o que você criou)
3. Clique em **"Finish"**

### 6.4 Publicar

1. Na tela de publicação, você verá um resumo
2. Clique em **"Publish"** (Publicar)
3. ⏳ Aguarde o build e deploy (pode levar 2-5 minutos)

**✅ Quando terminar**, o Visual Studio abrirá o navegador automaticamente!

---

## PASSO 7: Verificar se Funcionou

### 7.1 Testar Aplicação

1. Acesse: `https://SEU-APP-NAME.azurewebsites.net`
2. Você deve ver a página inicial
3. Tente criar uma conta (Register)
4. Tente fazer login

### 7.2 Verificar Logs (se houver erro)

1. No Azure Portal, vá no seu App Service
2. No menu lateral, vá em **"Log stream"**
3. Veja os logs em tempo real

### 7.3 Verificar Migrations

As migrations devem ser aplicadas automaticamente na primeira inicialização (já está configurado no `Program.cs`).

Se não funcionar, você pode aplicar manualmente via Kudu:
1. Acesse: `https://SEU-APP-NAME.scm.azurewebsites.net`
2. Vá em **"Debug console"** > **"CMD"**
3. Navegue até `site/wwwroot`
4. Execute: `dotnet ef database update` (se tiver EF tools instalado)

---

## PASSO 8: Configurar Email (Opcional mas Recomendado)

Para que os emails de confirmação funcionem em produção:

### Opção 1: SendGrid (Gratuito - 100 emails/dia)

1. Crie conta em: **https://sendgrid.com**
2. Vá em **Settings** > **API Keys**
3. Crie uma API Key
4. No Azure Portal, vá no seu App Service > **Configuration**
5. Adicione uma variável:
   - **Name**: `SendGrid__ApiKey`
   - **Value**: Sua API Key do SendGrid
6. Salve

### Opção 2: Desabilitar Email Confirmation (Temporário)

Se quiser testar sem configurar email, você pode temporariamente desabilitar:

1. No Azure Portal > App Service > **Configuration**
2. Adicione variável:
   - **Name**: `ASPNETCORE_ENVIRONMENT`
   - **Value**: `Development` (temporariamente)
3. Salve e reinicie

---

## ✅ Checklist Final

Marque conforme for completando:

- [ ] Conta Azure criada
- [ ] SQL Database criado
- [ ] Firewall do SQL Server configurado
- [ ] Connection string obtida e salva
- [ ] App Service criado (F1 FREE)
- [ ] Connection string configurada no App Service
- [ ] Projeto publicado do Visual Studio
- [ ] Aplicação acessível online
- [ ] Login/Registro funcionando
- [ ] Migrations aplicadas (automático)
- [ ] Dados de exemplo carregados (automático)

---

## 🎯 URLs Importantes

Guarde estas URLs:

- **Sua aplicação**: `https://SEU-APP-NAME.azurewebsites.net`
- **Portal Azure**: `https://portal.azure.com`
- **Kudu (Debug)**: `https://SEU-APP-NAME.scm.azurewebsites.net`
- **GitHub**: `https://github.com/Mjulys/Contratos2`

---

## 🆘 Problemas Comuns e Soluções

### Erro: "Cannot open server"
- **Causa**: Firewall do SQL Server não configurado
- **Solução**: Adicione regra `0.0.0.0 - 0.0.0.0` no firewall

### Erro: "Login failed"
- **Causa**: Senha incorreta na connection string
- **Solução**: Verifique se substituiu `{your_password}` pela senha real

### Erro 500 no site
- **Causa**: Migrations não aplicadas ou erro no código
- **Solução**: 
  1. Verifique logs em "Log stream"
  2. Verifique se connection string está correta
  3. As migrations devem aplicar automaticamente

### Site não carrega
- **Causa**: App Service pode estar parado
- **Solução**: No Azure Portal, vá no App Service e clique em "Start"

### Email não funciona
- **Causa**: Email sender não configurado
- **Solução**: Configure SendGrid ou desabilite temporariamente email confirmation

---

## 💰 Custos

### O que é GRÁTIS:
- ✅ App Service F1 tier (sempre grátis)
- ✅ SQL Database Basic (grátis por 12 meses)
- ✅ $200 em créditos por 30 dias

### Após 12 meses:
- SQL Database Basic: ~$5/mês (ou pode migrar para outro banco)
- App Service F1: Continua grátis!

---

## 🎉 Pronto!

Sua aplicação está no ar! Compartilhe o link com quem quiser testar.

**URL da sua aplicação**: `https://SEU-APP-NAME.azurewebsites.net`

---

## 📞 Precisa de Ajuda?

- Azure Docs: https://docs.microsoft.com/azure/app-service
- Azure Support: https://azure.microsoft.com/support/
- Stack Overflow: Tag `azure-app-service`

---

**Boa sorte com sua publicação! 🚀**


#!/usr/bin/env bash
set -euo pipefail

# Ensure we're in repo root
cd "$(dirname "$0")/.."

echo "==> Creating solution"
dotnet new sln -n FinPay

echo "==> Creating Shared libraries"
dotnet new classlib -n Contracts -o src/Shared/Contracts
dotnet new classlib -n BuildingBlocks -o src/Shared/BuildingBlocks

echo "==> Adding Shared to solution"
dotnet sln add src/Shared/Contracts/Contracts.csproj
dotnet sln add src/Shared/BuildingBlocks/BuildingBlocks.csproj

# Packages for shared libs
dotnet add src/Shared/BuildingBlocks/BuildingBlocks.csproj package Serilog.AspNetCore
dotnet add src/Shared/BuildingBlocks/BuildingBlocks.csproj package Serilog.Sinks.Seq
dotnet add src/Shared/BuildingBlocks/BuildingBlocks.csproj package OpenTelemetry.Extensions.Hosting
dotnet add src/Shared/BuildingBlocks/BuildingBlocks.csproj package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add src/Shared/BuildingBlocks/BuildingBlocks.csproj package MassTransit
dotnet add src/Shared/BuildingBlocks/BuildingBlocks.csproj package MassTransit.RabbitMQ

echo "==> Creating Services"
services=(ApiGateway AuthService MerchantService PaymentService FraudService NotificationService SettlementService)

for s in "${services[@]}"; do
  dotnet new webapi -n "$s" -o "src/Services/$s" --framework net8.0
  dotnet sln add "src/Services/$s/$s.csproj"
  dotnet add "src/Services/$s/$s.csproj" reference src/Shared/Contracts/Contracts.csproj
  dotnet add "src/Services/$s/$s.csproj" reference src/Shared/BuildingBlocks/BuildingBlocks.csproj

  # Common packages
  dotnet add "src/Services/$s/$s.csproj" package Pomelo.EntityFrameworkCore.MySql
  dotnet add "src/Services/$s/$s.csproj" package MediatR.Extensions.Microsoft.DependencyInjection
  dotnet add "src/Services/$s/$s.csproj" package Serilog.AspNetCore
  dotnet add "src/Services/$s/$s.csproj" package OpenTelemetry.Extensions.Hosting
  dotnet add "src/Services/$s/$s.csproj" package OpenTelemetry.Exporter.OpenTelemetryProtocol
  dotnet add "src/Services/$s/$s.csproj" package MassTransit
  dotnet add "src/Services/$s/$s.csproj" package MassTransit.RabbitMQ
done

# Gateway packages
dotnet add src/Services/ApiGateway/ApiGateway.csproj package Yarp.ReverseProxy

echo "==> Overwriting Program.cs and appsettings.json with templates"
for s in "${services[@]}"; do
  cp -f src/Services/$s/Program.cs src/Services/$s/Program.cs.bak || true
  cp -f src/Services/$s/appsettings.json src/Services/$s/appsettings.json.bak || true
done

# Copy prepared templates already in repo (created by this starter)
# (They are currently in the same paths; so no extra copy needed.)

echo "==> Writing ApiGateway YARP config hint"
echo 'builder.Configuration.AddJsonFile("appsettings.yarp.json", optional: true);' >> src/Services/ApiGateway/Program.cs

echo "==> Done. Next:"
echo "   1) docker compose -f deploy/docker-compose.yml up -d"
echo "   2) dotnet build"
echo "   3) dotnet run --project src/Services/PaymentService/PaymentService.csproj"

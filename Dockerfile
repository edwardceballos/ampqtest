FROM registry.access.redhat.com/ubi8/dotnet-80-runtime AS base
WORKDIR /app
USER 0
EXPOSE 8080
FROM registry.access.redhat.com/ubi8/dotnet-80 AS build
USER 0
WORKDIR /src
COPY . .
RUN dotnet restore "AmpqTest.csproj"
WORKDIR "/src/"
RUN dotnet build "AmpqTest.csproj" -c release -o /app/build

FROM build AS publish
RUN dotnet publish "AmpqTest.csproj" -c release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AmpqTest.dll"]
USER 1001

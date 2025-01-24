ARG buildImage
ARG runtimeImage

FROM ${buildImage} as publish
WORKDIR ${APP_ROOT}
COPY --chown=${USER} ["./MyApi.WebApi.csproj", "src/MyApi.WebApi/"]
RUN dotnet restore "src/MyApi.WebApi/MyApi.WebApi.csproj"
COPY --chown=${USER} [".", "src/MyApi.WebApi/"]
RUN dotnet publish "src/MyApi.WebApi/MyApi.WebApi.csproj" -c Release -o ${APP_ROOT}/app/publish --runtime linux-x64 --no-self-contained

FROM ${runtimeImage} AS final
WORKDIR ${APP_ROOT}/app
COPY --from=publish ["${APP_ROOT}/app/publish", "."]
ENTRYPOINT ["dotnet", "MyApi.WebApi.dll"]
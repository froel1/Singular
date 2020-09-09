FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

EXPOSE 80

ENV INTEGRATION_LABEL=egt
ENV INTEGRATION_TYPE=game

COPY /bin/publish /app/

WORKDIR /app

ENTRYPOINT ["dotnet", "Singular.CorePlatform.Game.EGT.dll"]
ARG DOT_NET_RUN_TAG=${DOT_NET_RUN_TAG}
FROM mcr.microsoft.com/dotnet/core/runtime:${DOT_NET_RUN_TAG} AS base
WORKDIR /app

ARG DOT_NET_SDK_TAG=${DOT_NET_SDK_TAG}
ARG ARCH_CONST=${ARCH_CONST}
FROM mcr.microsoft.com/dotnet/core/sdk:${DOT_NET_SDK_TAG} AS build
ARG ARCH_CONST
WORKDIR /src
COPY ["fanuc/fanuc.csproj", "fanuc/"]
RUN dotnet restore "fanuc/fanuc.csproj"
COPY . .
WORKDIR "/src/fanuc"
RUN dotnet build "fanuc.csproj" \
	-c Release \
	-o /app/build \
	/nowarn:CS0618 \
	/nowarn:CS8632 \
	/nowarn:CS1998 \
	/nowarn:CS8032 \
	-p:DefineConstants=${ARCH_CONST}
FROM build AS publish
RUN dotnet publish "fanuc.csproj" \
	-c Release \
	-o /app/publish \
	/nowarn:CS0618 \
	/nowarn:CS8632 \
	/nowarn:CS1998 \
	/nowarn:CS8032 \
	-p:DefineConstants=${ARCH_CONST}

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /etc/fanuc
COPY examples/fanuc-driver/config-example.yml /etc/fanuc/nlog.config
COPY examples/fanuc-driver/nlog-example-linux.config /etc/fanuc/nlog.config
EXPOSE 7800-7900
ENTRYPOINT ["dotnet", "fanuc.dll", "--nlog", "/etc/fanuc/nlog.config", "--config", "/etc/fanuc/config.yml"]

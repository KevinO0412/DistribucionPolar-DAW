#!/usr/bin/env bash
# Ejecuta las pruebas unitarias con coverlet y genera el reporte HTML de cobertura con ReportGenerator.
#   scripts/cobertura.sh            → docs/evidencias/cobertura/index.html
# Requiere el SDK de .NET 10 (o ejecutarlo dentro de mcr.microsoft.com/dotnet/sdk:10.0).
set -euo pipefail
cd "$(dirname "$0")/.."

RESULTS=TestResults/coverage
OUTPUT=docs/evidencias/cobertura
UNIT_TESTS=(Core.Domain.Tests Core.Application.Tests)

rm -rf "$RESULTS" "$OUTPUT"
dotnet tool restore >/dev/null
for project in "${UNIT_TESTS[@]}"; do
  dotnet test "$project" --configuration Release --collect "XPlat Code Coverage" --settings coverlet.runsettings \
    --results-directory "$RESULTS" --logger "trx;LogFileName=$(basename "$project").trx"
done

dotnet reportgenerator -reports:"$RESULTS/**/coverage.cobertura.xml" -targetdir:"$OUTPUT" \
  -reporttypes:"HtmlInline;TextSummary;MarkdownSummaryGithub" -title:"Distribución Polar: pruebas unitarias"
cat "$OUTPUT/Summary.txt" | head -n 20

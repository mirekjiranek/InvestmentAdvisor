using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestmentInstruments",
                columns: table => new
                {
                    InvestmentInstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentInstruments", x => x.InvestmentInstrumentId);
                });

            migrationBuilder.CreateTable(
                name: "Fundamentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestmentInstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Valuation_PE = table.Column<decimal>(type: "numeric", nullable: false),
                    Valuation_PB = table.Column<decimal>(type: "numeric", nullable: false),
                    Valuation_EVEBITDA = table.Column<decimal>(type: "numeric", nullable: false),
                    Valuation_EVEBIT = table.Column<decimal>(type: "numeric", nullable: false),
                    Valuation_PriceSales = table.Column<decimal>(type: "numeric", nullable: false),
                    Valuation_PriceCashFlow = table.Column<decimal>(type: "numeric", nullable: false),
                    Growth_HistoricalEps = table.Column<decimal>(type: "numeric", nullable: false),
                    Growth_PredictedEps = table.Column<decimal>(type: "numeric", nullable: false),
                    Growth_Revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    Growth_Profit = table.Column<decimal>(type: "numeric", nullable: false),
                    Growth_Dividend = table.Column<decimal>(type: "numeric", nullable: false),
                    Growth_PredictedFCF = table.Column<decimal>(type: "numeric", nullable: false),
                    Growth_LongTerm = table.Column<decimal>(type: "numeric", nullable: false),
                    Profitability_ROE = table.Column<decimal>(type: "numeric", nullable: false),
                    Profitability_ROA = table.Column<decimal>(type: "numeric", nullable: false),
                    Profitability_GrossMargin = table.Column<decimal>(type: "numeric", nullable: false),
                    Profitability_OperatingMargin = table.Column<decimal>(type: "numeric", nullable: false),
                    Profitability_NetMargin = table.Column<decimal>(type: "numeric", nullable: false),
                    Stability_DebtToEquity = table.Column<decimal>(type: "numeric", nullable: false),
                    Stability_CurrentRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    Stability_QuickRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    Stability_InterestCoverage = table.Column<decimal>(type: "numeric", nullable: false),
                    Dividend_Yield = table.Column<decimal>(type: "numeric", nullable: false),
                    Dividend_PayoutRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    Dividend_Growth = table.Column<decimal>(type: "numeric", nullable: false),
                    Dividend_CurrentAnnual = table.Column<decimal>(type: "numeric", nullable: false),
                    MarketRisk_Beta = table.Column<decimal>(type: "numeric", nullable: false),
                    MarketRisk_SharpeRatio = table.Column<decimal>(type: "numeric", nullable: false),
                    MarketRisk_StdDev = table.Column<decimal>(type: "numeric", nullable: false),
                    Sentiment_TargetPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Sentiment_AnalystRec = table.Column<string>(type: "text", nullable: false),
                    Sentiment_MediaScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Comp_SectorAvgPE = table.Column<decimal>(type: "numeric", nullable: false),
                    Comp_SectorMedPB = table.Column<decimal>(type: "numeric", nullable: false),
                    Comp_PeerEVEBITDA = table.Column<decimal>(type: "numeric", nullable: false),
                    Comp_SectorPriceSales = table.Column<decimal>(type: "numeric", nullable: false),
                    Earnings_EPS = table.Column<decimal>(type: "numeric", nullable: false),
                    Earnings_EBITDA = table.Column<decimal>(type: "numeric", nullable: false),
                    Revenue_SalesPerShare = table.Column<decimal>(type: "numeric", nullable: false),
                    CashFlow_CurrentFCF = table.Column<decimal>(type: "numeric", nullable: false),
                    CashFlow_ProjectedFCFGrowth = table.Column<decimal>(type: "numeric", nullable: false),
                    CostOfCapital_WACC = table.Column<decimal>(type: "numeric", nullable: false),
                    CostOfCapital_RROE = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fundamentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fundamentals_InvestmentInstruments_InvestmentInstrumentId",
                        column: x => x.InvestmentInstrumentId,
                        principalTable: "InvestmentInstruments",
                        principalColumn: "InvestmentInstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceDataSet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestmentInstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Open = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    High = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Low = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Close = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceDataSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceDataSet_InvestmentInstruments_InvestmentInstrumentId",
                        column: x => x.InvestmentInstrumentId,
                        principalTable: "InvestmentInstruments",
                        principalColumn: "InvestmentInstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestmentInstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    TimeHorizon = table.Column<string>(type: "text", nullable: false),
                    TargetPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Rationale = table.Column<string>(type: "text", nullable: false),
                    RiskLevel = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_InvestmentInstruments_InvestmentInstrumentId",
                        column: x => x.InvestmentInstrumentId,
                        principalTable: "InvestmentInstruments",
                        principalColumn: "InvestmentInstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fundamentals_InvestmentInstrumentId",
                table: "Fundamentals",
                column: "InvestmentInstrumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDataSet_InvestmentInstrumentId",
                table: "PriceDataSet",
                column: "InvestmentInstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_InvestmentInstrumentId",
                table: "Recommendations",
                column: "InvestmentInstrumentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fundamentals");

            migrationBuilder.DropTable(
                name: "PriceDataSet");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "InvestmentInstruments");
        }
    }
}

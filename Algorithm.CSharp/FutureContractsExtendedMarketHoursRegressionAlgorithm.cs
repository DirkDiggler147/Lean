/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Securities.Future;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This regression algorithm asserts that futures have data at extended market hours when this is enabled.
    /// </summary>
    public class FutureContractsExtendedMarketHoursRegressionAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Future _es;
        private Future _gc;
        private bool _esRanOnRegularHours;
        private bool _esRanOnExtendedHours;
        private bool _gcRanOnRegularHours;
        private bool _gcRanOnExtendedHours;

        public override void Initialize()
        {
            SetStartDate(2013, 10, 6);
            SetEndDate(2013, 10, 11);

            var esFutureSymbol = QuantConnect.Symbol.CreateFuture(Futures.Indices.SP500EMini, Market.CME, new DateTime(2013, 12, 20));
            _es = AddFutureContract(esFutureSymbol, Resolution.Hour, fillDataForward: true, extendedMarketHours: true);

            var gcFutureSymbol = QuantConnect.Symbol.CreateFuture(Futures.Metals.Gold, Market.COMEX, new DateTime(2013, 10, 29));
            _gc = AddFutureContract(gcFutureSymbol, Resolution.Hour, fillDataForward: true, extendedMarketHours: false);
        }

        public override void OnData(Slice slice)
        {
            var sliceSymbols = new HashSet<Symbol>(slice.Keys);
            sliceSymbols.UnionWith(slice.Bars.Keys);
            sliceSymbols.UnionWith(slice.Ticks.Keys);
            sliceSymbols.UnionWith(slice.QuoteBars.Keys);

            var esIsInRegularHours = _es.Exchange.Hours.IsOpen(Time, false);
            var esIsInExtendedHours = !esIsInRegularHours && _es.Exchange.Hours.IsOpen(Time, true);
            var sliceHasESData = sliceSymbols.Any(symbol => symbol == _es.Symbol || symbol.Canonical == _es.Symbol);
            _esRanOnRegularHours |= esIsInRegularHours && sliceHasESData;
            _esRanOnExtendedHours |= esIsInExtendedHours && sliceHasESData;

            var gcIsInRegularHours = _gc.Exchange.Hours.IsOpen(Time, false);
            var gcIsInExtendedHours = !gcIsInRegularHours && _gc.Exchange.Hours.IsOpen(Time, true);
            var sliceHasGCData = sliceSymbols.Any(symbol => symbol == _gc.Symbol || symbol.Canonical == _gc.Symbol);
            _gcRanOnRegularHours |= gcIsInRegularHours && sliceHasGCData;
            _gcRanOnExtendedHours |= gcIsInExtendedHours && sliceHasGCData;
        }

        public override void OnEndOfAlgorithm()
        {
            if (!_esRanOnRegularHours)
            {
                throw new Exception($"Algorithm should have run on regular hours for {_es.Symbol} future, which enabled extended market hours");
            }

            if (!_esRanOnExtendedHours)
            {
                throw new Exception($"Algorithm should have run on extended hours for {_es.Symbol} future, which enabled extended market hours");
            }

            if (!_gcRanOnRegularHours)
            {
                throw new Exception($"Algorithm should have run on regular hours for {_gc.Symbol} future, which did not enable extended market hours");
            }

            if (_gcRanOnExtendedHours)
            {
                throw new Exception($"Algorithm should have not run on extended hours for {_gc.Symbol} future, which did not enable extended market hours");
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 525;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "0"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "0%"},
            {"Drawdown", "0%"},
            {"Expectancy", "0"},
            {"Net Profit", "0%"},
            {"Sharpe Ratio", "0"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0"},
            {"Annual Variance", "0"},
            {"Information Ratio", "-2.564"},
            {"Tracking Error", "0.214"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "$0.00"},
            {"Estimated Strategy Capacity", "$0"},
            {"Lowest Capacity Asset", ""},
            {"Portfolio Turnover", "0%"},
            {"OrderListHash", "d41d8cd98f00b204e9800998ecf8427e"}
        };
    }
}

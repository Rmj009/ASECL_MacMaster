using MT8872AFIXTURE.CSV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT8872AFIXTURE.LCS5
{
    public abstract class CsvBase<TEST_ITEM> : ICsv
    {
        protected abstract void _InitResultItemLimitUpperAndLower();

        protected abstract string _AliasHeaderName(string srcHeaderName);

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void SetCsvTempFilePath(string tempPath)
        {
            throw new NotImplementedException();
        }

        public void SetCsvReleaseFilePath(string releasePath)
        {
            throw new NotImplementedException();
        }

        public void SetIgnoreResultTitle(string item)
        {
            throw new NotImplementedException();
        }

        public void AddTestResultItem(object item)
        {
            throw new NotImplementedException();
        }

        public void ChangeLastResultErrorCodeAndException(string errorCode, string err)
        {
            throw new NotImplementedException();
        }

        public void ChangeLastResultTrayData(string trayDutSN, string trayName, string trayX, string trayY)
        {
            throw new NotImplementedException();
        }

        public void SetBeginTestTime(string value)
        {
            throw new NotImplementedException();
        }

        public void SetEndTestTime(string value)
        {
            throw new NotImplementedException();
        }

        public void CalcFinalResult()
        {
            throw new NotImplementedException();
        }

        public void GetCalcFinishResult(ref double fYieldRate, ref int total, ref int pass, ref int fail)
        {
            throw new NotImplementedException();
        }

        public string GetLastResult()
        {
            throw new NotImplementedException();
        }

        public string GetResultTitleJSON()
        {
            throw new NotImplementedException();
        }

        public string GetResultLimitUpperJson()
        {
            throw new NotImplementedException();
        }

        public string GetResultLimitLowerJson()
        {
            throw new NotImplementedException();
        }

        public void DebugShowAll()
        {
            throw new NotImplementedException();
        }

        public void DeleteEmptyTestResultCsv()
        {
            throw new NotImplementedException();
        }

        public string TruncateUnderscore(string[] nomenclature)
        {
            throw new NotImplementedException();
        }
    }
}

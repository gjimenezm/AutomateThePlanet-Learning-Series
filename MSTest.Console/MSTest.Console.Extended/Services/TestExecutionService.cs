﻿// <copyright file="TestExecutionService.cs" company="Automate The Planet Ltd.">
// Copyright 2016 Automate The Planet Ltd.
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Anton Angelov</author>
// <site>http://automatetheplanet.com/</site>
using System.Collections.Generic;
using System.Linq;
using log4net;
using MSTest.Console.Extended.Data;
using MSTest.Console.Extended.Interfaces;

namespace MSTest.Console.Extended.Services
{
    public class TestExecutionService
    {
        private readonly ILog log;

        private readonly IMsTestTestRunProvider microsoftTestTestRunProvider;

        private readonly IFileSystemProvider fileSystemProvider;

        private readonly IProcessExecutionProvider processExecutionProvider;

        private readonly IConsoleArgumentsProvider consoleArgumentsProvider;

        public TestExecutionService(
            IMsTestTestRunProvider microsoftTestTestRunProvider,
            IFileSystemProvider fileSystemProvider,
            IProcessExecutionProvider processExecutionProvider,
            IConsoleArgumentsProvider consoleArgumentsProvider,
            ILog log)
        {
            this.microsoftTestTestRunProvider = microsoftTestTestRunProvider;
            this.fileSystemProvider = fileSystemProvider;
            this.processExecutionProvider = processExecutionProvider;
            this.consoleArgumentsProvider = consoleArgumentsProvider;
            this.log = log;
        }
        
        public int ExecuteWithRetry()
        {
            this.fileSystemProvider.DeleteTestResultFiles();
            this.processExecutionProvider.ExecuteProcessWithAdditionalArguments();
            this.processExecutionProvider.CurrentProcessWaitForExit();
            var testRun = this.fileSystemProvider.DeserializeTestRun();
            int areAllTestsGreen = 0;
            var failedTests = new List<TestRunUnitTestResult>();
            failedTests = this.microsoftTestTestRunProvider.GetAllNotPassedTests(testRun.Results.ToList());
            int failedTestsPercentage = this.microsoftTestTestRunProvider.CalculatedFailedTestsPercentage(failedTests, testRun.Results.ToList());
            if (failedTestsPercentage < this.consoleArgumentsProvider.FailedTestsThreshold)
            {
                for (int i = 0; i < this.consoleArgumentsProvider.RetriesCount - 1; i++)
                {
                    this.log.InfoFormat("Start to execute again {0} failed tests.", failedTests.Count);
                    if (failedTests.Count > 0)
                    {
                        string currentTestResultPath = this.fileSystemProvider.GetTempTrxFile();
                        string retryRunArguments = this.microsoftTestTestRunProvider.GenerateAdditionalArgumentsForFailedTestsRun(failedTests, currentTestResultPath);
                   
                        this.log.InfoFormat("Run {0} time with arguments {1}", i + 2, retryRunArguments);
                        this.processExecutionProvider.ExecuteProcessWithAdditionalArguments(retryRunArguments);
                        this.processExecutionProvider.CurrentProcessWaitForExit();
                        var currentTestRun = this.fileSystemProvider.DeserializeTestRun(currentTestResultPath);
                        var passedTests = this.microsoftTestTestRunProvider.GetAllPassesTests(currentTestRun);
                        this.microsoftTestTestRunProvider.UpdatePassedTests(passedTests, testRun.Results.ToList());
                        this.microsoftTestTestRunProvider.UpdateResultsSummary(testRun);
                    }
                    else
                    {
                        break;
                    }
                    failedTests = this.microsoftTestTestRunProvider.GetAllNotPassedTests(testRun.Results.ToList());
                }
            }
            else
            {
                this.log.InfoFormat("Percentage of failed tests {0} is over threshold {1}, will not restart", failedTestsPercentage, this.consoleArgumentsProvider.FailedTestsThreshold);
            }
            if (failedTests.Count > 0)
            {
                areAllTestsGreen = 1;
            }
            this.fileSystemProvider.SerializeTestRun(testRun);

            return areAllTestsGreen;
        }
    }
}
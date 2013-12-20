using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

/* 

//USAGE:

RXPerformanceTester.testA = () =>
{
	Math.Sin(0.0);
};

RXPerformanceTester.testB = () =>
{
	Math.Cos(0.0);
};

RXPerformanceTester.Run(25,10000);

*/

public class RXPerformanceTester
{
	static public Action<string> Log = UnityEngine.Debug.Log;
	//static public Action<string> Log = Console.WriteLine; //use this if you want no reliance on Unity

	static public Action testA;
	static public Action testB;

	static public bool isRunning = false;

	static private int _numTests;
	static private int _numIterationsPerTest;

	static private int _currentTestIndex;
	static private double _efficiency;

	static private Stopwatch _watch = new Stopwatch();

	//disable warnings so we don't get an "assigned but never used" warning
	//the _timer variable is used to store a reference to the timer so it doesn't get GC'd
	#pragma warning disable
	static private Timer _timer;
	#pragma warning restore

	//numtests should be a relatively low number, like 25, and numIterationsPerTest should be a high number like 1000, 10000, or 100000
	static public void Run(int numTests, int numIterationsPerTest)
	{
		if(isRunning)
		{
			Log("You must let the current test finish before running another");
			return;
		}

		isRunning = true;

		_numTests = numTests;
		_numIterationsPerTest = numIterationsPerTest;

		if(testA == null || testB == null) throw new Exception("RXPerformanceTester: You must set testA and testB before calling Run()");

		_currentTestIndex = -2; //-2 means two warm up tests
		_efficiency = 0.0f;

		DoNextTest();
	}

	static private void DoNextTest ()
	{
		//warm up tests for indexes below zero aren't counted
		if(_currentTestIndex < 0)
		{
			DoTest(_currentTestIndex);
		}
		else //do the test for real and measure the efficiency
		{
			_efficiency += DoTest(_currentTestIndex);
		}

		_currentTestIndex++;
		if(_currentTestIndex == _numTests)
		{
			DoFinalOutput();
		}
		else 
		{
			//delay for just a tiny amount of time so that it doesn't block the thread completely
			_timer = new System.Threading.Timer(obj => { DoNextTest(); }, null, 1, System.Threading.Timeout.Infinite);
		}
	}

	static private void DoFinalOutput ()
	{
		_efficiency /= (float)_numTests;
		
		if(Math.Abs(_efficiency*100.0 - 100.0) <= 2.0) //within 2 percent is considered equal
		{
			Log("All tests complete, they're equal!");
		}
		else if(_efficiency < 1.0)
		{
			int percent = (int) Math.Round(1.0/_efficiency * 100.0) - 100;
			Log("All tests complete, A is "+percent+"% faster!");
		}
		else 
		{
			int percent = (int) Math.Round(_efficiency * 100.0) - 100;
			Log("All tests complete, B is "+percent+"% faster!");
		}

		isRunning = false;
		_timer = null;
	}
	
	static private double DoTest(int testIndex)
	{
		long timeA = 0;
		long timeB = 0;

		//start with either test A or test B, randomly
		bool shouldTestABeFirst = new System.Random((int)DateTime.UtcNow.Ticks).Next() % 2 == 0;

		if(shouldTestABeFirst)
		{
			for(int t = 0; t<_numIterationsPerTest; t++)
			{
				_watch.Reset();
				_watch.Start();
				testA();
				timeA += _watch.ElapsedTicks;
			}

			for(int t = 0; t<_numIterationsPerTest; t++)
			{
				_watch.Reset();
				_watch.Start();
				testB();
				timeB += _watch.ElapsedTicks;
			}
		}
		else 
		{
			for(int t = 0; t<_numIterationsPerTest; t++)
			{
				_watch.Reset();
				_watch.Start();
				testB();
				timeB += _watch.ElapsedTicks;
			}

			for(int t = 0; t<_numIterationsPerTest; t++)
			{
				_watch.Reset();
				_watch.Start();
				testA();
				timeA += _watch.ElapsedTicks;
			}
		}

		double delta = (double)timeA/(double)timeB; 

		if(testIndex >= 0) //don't bother logging the warm up tests
		{
			Log("Test " + testIndex + " A:" + timeA + "   B:" + timeB + " efficiency: " + delta);
		}
		
		return delta;
	}
}
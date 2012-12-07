using System;

public class FutileException : Exception
{
	public FutileException (string message) : base(message)
	{
	}
	
	public FutileException () : base()
	{
	}
}


using System;
using System.Globalization;

/* This file, along with ImagePropertyTag.cs and Fraction.cs, are modified versions of the project posted 
 * at http://www.codeproject.com/dotnet/ImageInfo.asp, which was inspired by the earlier project 
 * http://www.codeproject.com/cs/media/photoproperties.asp.
 * Thanks to George Mamaladze and Jeffrey S. Gangel for their hard work. */

namespace GalleryServer.Business
{
	///<summary>
	/// Represents a fractional number by storing a distinct numerator and denominator. Contains static methods
	/// to provide functionality for basic arithmetic operations on the fraction.</summary>
	public class Fraction
	{
		#region Private Fields

		private System.Int64 _numerator;
		private System.Int64 _denominator;

		#endregion

		#region Constructors

		///<summary>
		/// Creates a Fraction Number having only a numerator and assuming denumerator = 1.
		/// </summary>
		///<param name="numerator">A System.Int64 representing the numerator of the fraction.</param>
		public Fraction(System.Int64 numerator) : this(numerator, 1) { }

		///<summary>
		/// Creates a Fraction Number having a numerator and denumerator.
		/// </summary>
		///<param name="numerator">A System.Int64 representing the numerator of the fraction.</param>
		///<param name="denominator">A System.Int64 representing the denominator of the fraction.</param>
		public Fraction(System.Int64 numerator, System.Int64 denominator)
		{
			this._numerator = numerator;
			this._denominator = denominator;
		}

		#endregion

		/// <summary>
		/// Provides a string representation of the fraction in the format "numerator/denominator" (i.e. "1/6"),
		/// reduced to its lowest possible value. For example, if the original fraction is 10/60, this method returns "1/6".
		/// Whole numbers where the denominator is 1 are shown without the slash or
		/// denominator (i.e. "12" instead of "12/1").
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> representation of this instance in the format "numerator/denominator" (i.e. "1/6").
		/// </returns>
		public override string ToString()
		{
			if (_denominator == 0)
			{
				return String.Empty;
			}
			else if (_denominator == 1)
			{
				return String.Format(CultureInfo.CurrentCulture, "{0}", _numerator);
			}
			else
			{
				long gcd = GetGreatestCommonDenominator();

				if (gcd == 0) return String.Empty;

				if ((_denominator > 0) && ((_denominator / gcd) == 1))
				{
					// Our reduction results in a whole number, so just output that (ex: 500/100 => "5")
					return (_numerator / gcd).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					return String.Format(CultureInfo.CurrentCulture, "{0}/{1}", _numerator / gcd, _denominator / gcd);
				}
			}
		}

		/// <summary>
		/// Returns this fraction in its decimal form.
		/// </summary>
		/// <returns>Returns this fraction in its decimal form.</returns>
		public float ToSingle()
		{
			if (_denominator == 0)
				return 0;

			return (float)_numerator / (float)_denominator;
		}

		/// <summary>
		/// Gets or sets the numerator of this fraction.
		/// </summary>
		/// <value>The numerator of this fraction.</value>
		public System.Int64 Numerator
		{
			get { return _numerator; }
			set { _numerator = value; }
		}

		/// <summary>
		/// Gets or sets the denominator of this fraction.
		/// </summary>
		/// <value>The denominator of this fraction.</value>
		public System.Int64 Denominator
		{
			get { return _denominator; }
			set { _denominator = value; }
		}

		/// <summary>
		/// Overloads the plus (+) operator.
		/// </summary>
		/// <param name="fracA">A Fraction to add to fracB.</param>
		/// <param name="fracB">A Fraction to add to fracA.</param>
		/// <returns>The result of the addition of fracA and fracB.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="fracA" /> or <paramref name="fracB" /> is null.</exception>
		public static Fraction operator +(Fraction fracA, Fraction fracB)
		{
			if (fracA == null)
				throw new ArgumentNullException("fracA");

			if (fracB == null)
				throw new ArgumentNullException("fracB");
			
			return new Fraction(fracA._numerator * fracB._denominator + fracB._numerator * fracA._denominator, fracA._denominator * fracB._denominator);
		}

		/// <summary>
		/// Overloads the multiplication (*) operator.
		/// </summary>
		/// <param name="fracA">A Fraction to multiply with fracB.</param>
		/// <param name="fracB">A Fraction to multiply with fracA.</param>
		/// <returns>The result of the multiplication of fracA and fracB.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="fracA" /> or <paramref name="fracB" /> is null.</exception>
		public static Fraction operator *(Fraction fracA, Fraction fracB)
		{
			if (fracA == null)
				throw new ArgumentNullException("fracA");

			if (fracB == null)
				throw new ArgumentNullException("fracB");
			
			return new Fraction(fracA._numerator * fracB._numerator, fracA._denominator * fracB._denominator);
		}

		/// <summary>
		/// Overloads the casting of this fraction to a <see cref="double" />. This is performed by carrying out the implicit
		/// division of the fraction. For example, the fraction 1/2 is cast to 0.50.
		/// </summary>
		/// <param name="frac">The Fraction to cast to a <see cref="double" />.</param>
		/// <returns>The result of the casting of this instance to a <see cref="double" />.</returns>
		public static implicit operator double(Fraction frac)
		{
			if ((frac == null) || (frac._denominator == 0))
				return 0;

			return ((double)frac._numerator) / ((double)frac._denominator);
		}

		/// <summary>
		/// Calculate the greatest common denominator for the numerator and denominator in this instance. This function uses the 
		/// well-known Euclidean Algorithm for generating the value.
		/// </summary>
		/// <returns>Returns the greatest common denominator.</returns>
		private long GetGreatestCommonDenominator()
		{
			return GetGreatestCommonDenominator(_numerator, _denominator);
		}

		/// <summary>
		/// Calculate the greatest common denominator for the two integers. This function uses the well-known Euclidean Algorithm
		/// for generating the value.
		/// </summary>
		/// <param name="intA">One of the integers for which the greatest common denominator is to be found.</param>
		/// <param name="intB">One of the integers for which the greatest common denominator is to be found.</param>
		/// <returns>Returns the greatest common denominator for the two integers.</returns>
		private static long GetGreatestCommonDenominator(long intA, long intB)
		{
			while (intA != 0 && intB != 0)
			{
				if (intA > intB)
					intA %= intB;
				else
					intB %= intA;
			}

			if (intA == 0)
				return intB;
			else
				return intA;
		}
	}
}

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace System.Text.Json.Converter
{
    /// <summary>
    /// Converts time intervals from convenient string format to system TimeSpan
    /// Be aware, it is implemented for deserialization or read only!
    /// Possible delimeters: - _:/'
    /// Examples: "1h34m26s134ms"; "1h 12m127s"; "1d/12h 15m 7s"; "3d/12h_15m:12s---347ms"
    /// </summary>
    public class TimeIntervalConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options )
        {
            var token = reader.GetString()
                ?? throw new ArgumentNullException( nameof( reader ) );
            TimeSpan interval = TimeInterval.Parse( token );

            return interval;
        }

        public override void Write(
            Utf8JsonWriter writer,
            TimeSpan value,
            JsonSerializerOptions options )
        {
            throw new NotImplementedException();
        }
    }

    [TypeConverter( typeof( TimeIntervalTypeConverter ) )]
    public class TimeInterval
    {
        public readonly TimeSpan Value;

        public TimeInterval( TimeSpan tspan )
        {
            Value = tspan;
        }

        public static TimeInterval By( TimeSpan tspan )
            => new( tspan );

        public static implicit operator TimeSpan( TimeInterval interval )
            => interval.Value;

        public static implicit operator TimeInterval( TimeSpan timeSpan )
            => By( timeSpan );

        public static readonly Regex Filter =
            new( @"(\d+)([dhms]+)[- _:\/']*",
                RegexOptions.IgnoreCase
                | RegexOptions.Singleline,
                TimeSpan.FromMilliseconds( 100 ) );

        public enum Unit { None, Day, Hour, Minute, Second, Millisecond }

        public static string UngroupInterval( GroupCollection groups )
            => groups[1].Value;
        public static string UngroupUnitName( GroupCollection groups )
            => groups[2].Value.Trim();

        public static Unit GetUnits( string name )
            => name switch {
                "d" => Unit.Day,
                "h" => Unit.Hour,
                "m" => Unit.Minute,
                "s" => Unit.Second,
                "ms" => Unit.Millisecond,
                _ => Unit.None
            };

        public static TimeSpan ToTimeSpan( double interval, Unit unit )
            => unit switch {
                Unit.Day => TimeSpan.FromDays( interval ),
                Unit.Hour => TimeSpan.FromHours( interval ),
                Unit.Minute => TimeSpan.FromMinutes( interval ),
                Unit.Second => TimeSpan.FromSeconds( interval ),
                Unit.Millisecond => TimeSpan.FromMilliseconds( interval ),
                _ => throw new ArgumentOutOfRangeException( nameof( unit ) ),
            };

        public static TimeSpan Parse( string token )
        {
            TimeSpan result = TimeSpan.Zero;

            var classicTimespan =
                token.Contains( ':' )
                && TimeSpan.TryParse( token, out result );

            if ( classicTimespan ) return result;

            var matches = Filter.Matches( token );

            foreach ( Match match in matches ) {
                double interval =
                    double.Parse(
                        UngroupInterval( match.Groups ) );

                var units =
                    GetUnits(
                        UngroupUnitName( match.Groups ) );

                result += ToTimeSpan( interval, units );
            }

            return result;
        }
    }

    public class TimeIntervalTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
            => sourceType == typeof( string )
            || base.CanConvertFrom( context, sourceType );

        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value )
        {
            if ( value is string token ) {
                try {
                    var timeSpan = TimeInterval.Parse( token );
                    return TimeInterval.By( timeSpan );
                }
                catch { }
            }

            return
                base.ConvertFrom( context, culture, value );
        }
    }
}

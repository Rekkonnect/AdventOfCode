using AdventOfCode.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Problems.Year2020
{
    public class Day4 : Problem2<int>
    {
        public override int SolvePart1()
        {
            return AnalyzePassports(p => p.HasRequiredFields);
        }
        public override int SolvePart2()
        {
            return AnalyzePassports(p => p.IsValid);
        }

        private int AnalyzePassports(Func<Passport, bool> predicate)
        {
            return NormalizedFileContents.Split("\n\n").Select(Passport.Parse).ToList().Count(predicate);
        }

        private class Passport
        {
            private readonly Dictionary<string, string> fields = new();

            private static readonly string[] validityKeyChecks = { "byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid" };
            private static readonly string[] validEyeColors = { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" };

            #region Fields
            public int BirthYear => int.Parse(fields["byr"]);
            public int IssueYear => int.Parse(fields["iyr"]);
            public int ExpirationYear => int.Parse(fields["eyr"]);
            public string Height => fields["hgt"];
            public string HairColor => fields["hcl"];
            public string EyeColor => fields["ecl"];
            public string PassportID => fields["pid"];
            #endregion

            public bool HasRequiredFields
            {
                get
                {
                    var keys = fields.Keys;
                    foreach (var keyCheck in validityKeyChecks)
                        if (!keys.Contains(keyCheck))
                            return false;
                    return true;
                }
            }
            public bool HasValidValues
            {
                get
                {
                    return IsValidBirthYear
                        && IsValidIssueYear
                        && IsValidExpirationYear
                        && IsValidHeight
                        && IsValidHairColor
                        && IsValidEyeColor
                        && IsValidPassportID;
                }
            }
            public bool IsValid => HasRequiredFields && HasValidValues;

            #region Field Validity Checks
            private bool IsValidBirthYear => IsWithin(BirthYear, 1920, 2002);
            private bool IsValidIssueYear => IsWithin(IssueYear, 2010, 2020);
            private bool IsValidExpirationYear => IsWithin(ExpirationYear, 2020, 2030);
            private bool IsValidHeight
            {
                get
                {
                    var stringValue = Height;

                    bool validValue = int.TryParse(stringValue[..^2], out int value);
                    if (!validValue)
                        return false;

                    if (stringValue.EndsWith("cm"))
                        return IsWithin(value, 150, 193);
                    if (stringValue.EndsWith("in"))
                        return IsWithin(value, 59, 76);
                    
                    return false;
                }
            }
            private bool IsValidHairColor
            {
                get
                {
                    var stringValue = HairColor;

                    if (!stringValue.StartsWith('#'))
                        return false;

                    if (stringValue.Length != 7)
                        return false;

                    var hex = stringValue[1..];
                    return !hex.Any(c => !c.IsValidHexCharacter());
                }
            }
            private bool IsValidEyeColor => validEyeColors.Contains(EyeColor);
            private bool IsValidPassportID
            {
                get
                {
                    var field = PassportID;
                    return field.Length == 9 && !field.Any(c => !char.IsDigit(c));
                }
            }
            #endregion

            private bool IsWithin(int value, int min, int max)
            {
                return min <= value && value <= max;
            }

            public static Passport Parse(string passportRaw)
            {
                var result = new Passport();

                passportRaw = passportRaw.Replace('\n', ' ');
                var fields = passportRaw.Split(' ');

                foreach (var field in fields)
                {
                    var split = field.Split(':');
                    result.fields.Add(split[0], split[1]);
                }

                return result;
            }
        }
    }
}

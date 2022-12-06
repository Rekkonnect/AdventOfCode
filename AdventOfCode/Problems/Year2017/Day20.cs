using AdventOfCode.Utilities;
using AdventOfCode.Utilities.ThreeDimensions;
using AdventOfCSharp.Extensions;

namespace AdventOfCode.Problems.Year2017;

public partial class Day20 : Problem<int>
{
    private ParticleSystem particleSystem;

    public override int SolvePart1()
    {
        return particleSystem.ClosestParticleIndex;
    }
    public override int SolvePart2()
    {
        return particleSystem.GetCountAfterRemovingCollsions();
    }

    protected override void LoadState()
    {
        particleSystem = new(ParsedFileLines(Particle.Parse));
    }
    protected override void ResetState()
    {
        particleSystem = null;
    }

    private class ParticleSystem
    {
        private Particle[] particles;

        public int ClosestParticleIndex => particles.Select(p => p.Acceleration.ManhattanDistanceFromCenter).ToArray().MinIndex();

        public ParticleSystem(Particle[] particles)
        {
            this.particles = particles;
        }

        public int GetCountAfterRemovingCollsions()
        {
            var remaining = new HashSet<MovableParticle>(particles.Select(MovableParticle.FromParticle));
            int previousCount = remaining.Count;
            int ignoredParticles = 0;

            var locations = new FlexibleDictionary<Location3D, MovableParticle>();
            int nonCollidingRounds = 0;

            while (remaining.Any())
            {
                nonCollidingRounds++;

                // ToArray is sadly necessary
                foreach (var particle in remaining.ToArray())
                {
                    particle.Iterate();
                    var position = particle.Position;

                    // Identify collision and remove the colliding particles
                    if (!locations.TryAdd(position, particle))
                    {
                        remaining.Remove(locations[position]);
                        remaining.Remove(particle);
                        nonCollidingRounds = 0;
                    }
                }

                locations.Clear();

                const int collisionRoundThreshold = 12;

                // No collisions in the past rounds; must evaluate the relationships between the particles
                // to ignore the ones that will never collide
                if (nonCollidingRounds > collisionRoundThreshold)
                {
                    var approachMap = new FlexibleListDictionary<MovableParticle, MovableParticle>();

                    var remainingArray = remaining.ToArray();
                    for (int i = 0; i < remainingArray.Length; i++)
                    {
                        var a = remainingArray[i];

                        for (int j = i + 1; j < remainingArray.Length; j++)
                        {
                            var b = remainingArray[j];

                            if (!a.Approaches(b))
                                continue;

                            approachMap[a].Add(b);
                            approachMap[b].Add(a);
                        }
                    }

                    // Eliminate disconnected particles
                    foreach (var particle in remainingArray)
                    {
                        if (approachMap.ContainsKey(particle))
                            continue;

                        remaining.Remove(particle);
                        ignoredParticles++;
                    }

                    // Reset the counter since the particles were reduced
                    nonCollidingRounds = 0;
                }
            }

            return ignoredParticles;
        }
    }

    private class MovableParticle
    {
        private Location3D position;
        private Location3D velocity;
        private Location3D acceleration;

        public Location3D Position => position;
        public Location3D Velocity => velocity;
        public Location3D Acceleration => acceleration;

        public MovableParticle(Particle particle)
        {
            position = particle.Position;
            velocity = particle.Velocity;
            acceleration = particle.Acceleration;
        }

        public bool CollidesWith(MovableParticle other) => position == other.position;

        public bool Approaches(MovableParticle other)
        {
            // Is this actually correct?
            var positionDifference = position.SignedDifferenceFrom(other.position);
            var accelerationDifference = acceleration.SignedDifferenceFrom(other.acceleration);
            return positionDifference == accelerationDifference && acceleration + other.acceleration == Location3D.Zero;
        }

        public void Iterate()
        {
            velocity += acceleration;
            position += velocity;
        }

        public static MovableParticle FromParticle(Particle particle) => new(particle);

        public override string ToString()
        {
            return $"{position} {velocity} {acceleration}";
        }
    }

    private partial record Particle(Location3D Position, Location3D Velocity, Location3D Acceleration)
    {
        private static readonly Regex particlePattern = ParticleRegex();
        private static readonly Regex locationPattern = LocationRegex();

        [GeneratedRegex("p=(?'position'.*), v=(?'velocity'.*), a=(?'acceleration'.*)", RegexOptions.Compiled)]
        private static partial Regex ParticleRegex();
        [GeneratedRegex("\\<(?'x'[-\\d]*),(?'y'[-\\d]*),(?'z'[-\\d]*)\\>", RegexOptions.Compiled)]
        private static partial Regex LocationRegex();

        public static Particle Parse(string raw)
        {
            var groups = particlePattern.Match(raw).Groups;
            var position = ParseLocation(groups["position"].Value);
            var velocity = ParseLocation(groups["velocity"].Value);
            var acceleration = ParseLocation(groups["acceleration"].Value);
            return new(position, velocity, acceleration);
        }

        private static Location3D ParseLocation(string raw)
        {
            var groups = locationPattern.Match(raw).Groups;
            int x = groups["x"].Value.ParseInt32();
            int y = groups["y"].Value.ParseInt32();
            int z = groups["z"].Value.ParseInt32();
            return (x, y, z);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Documents;

namespace Superflat
{
    public struct XValue<T>
    {
        public bool HasValue { get; set; }
        public T Value { get; set; }

        public override string ToString()
        {
            return HasValue ? Value?.ToString() : null;
        }

        public static implicit operator XValue<T>(T value)
        {
            return new XValue<T>
            {
                HasValue = true,
                Value = value
            };
        }
    }

    public abstract class SetupBase
    {
        public abstract string SetupId { get; }
        public bool Enabled { get; set; }

        public string SetupString
        {
            get
            {
                var s = string.Join(" ", properties.Select(t => t()).Where(t => t != null));
                if (!string.IsNullOrEmpty(s))
                    s = $"({s})";
                return $"{SetupId}{s}";
            }
        }
        private List<Func<string>> properties = new List<Func<string>>();

        protected void RegisterProperty<T>(string name, Func<XValue<T>> p)
        {
            properties.Add(() =>
            {
                var v = p();
                return v.HasValue ? $"{name}={v.Value}" : null;
            });
        }
    }

    public class VillageSetup : SetupBase
    {
        public override string SetupId => "village";

        public XValue<int> Size { get; set; }
        public XValue<int> Distance { get; set; }

        public VillageSetup()
        {
            RegisterProperty("size", () => Size);
            RegisterProperty("distance", () => Distance);
        }
    }
    public class StrongholdSetup : SetupBase
    {
        public override string SetupId => "stronghold";

        public XValue<int> Count { get; set; }
        public XValue<float> Distance { get; set; }
        public XValue<float> Spread { get; set; }

        public StrongholdSetup()
        {
            RegisterProperty("size", () => Count);
            RegisterProperty("distance", () => Distance);
            RegisterProperty("spread", () => Spread);
        }
    }
    public class MineshaftSetup : SetupBase
    {
        public override string SetupId => "mineshaft";

        public XValue<float> Chance { get; set; }

        public MineshaftSetup()
        {
            RegisterProperty("chance", () => Chance);
        }
    }
    public class Biome1Setup : SetupBase
    {
        public override string SetupId => "biome_1";

        public XValue<int> Distance { get; set; }

        public Biome1Setup()
        {
            RegisterProperty("distance", () => Distance);
        }
    }
    public class OceanmonumentSetup : SetupBase
    {
        public override string SetupId => "oceanmonument";

        public XValue<int> Spacing { get; set; }
        public XValue<int> Separation { get; set; }

        public OceanmonumentSetup()
        {
            RegisterProperty("spacing", () => Spacing);
            RegisterProperty("separation", () => Separation);
        }
    }

    public class DungeonSetup : SetupBase
    {
        public override string SetupId => "dungeon";
    }

    public class DecorationSetup : SetupBase
    {
        public override string SetupId => "decoration";
    }

    public class LakeSetup : SetupBase
    {
        public override string SetupId => "lake";
    }

    public class LavaLakeSetup : SetupBase
    {
        public override string SetupId => "lava_lake";
    }

    public class ConfigBuilder
    {
        public Block[] Layers { get; set; } = Array.Empty<Block>();
        public bool BiomeEnabled { get; set; }
        public Biome Biome { get; set; }
        public VillageSetup Village { get; set; } = new VillageSetup();
        public StrongholdSetup Stronghold { get; set; } = new StrongholdSetup();
        public MineshaftSetup Mineshaft { get; set; } = new MineshaftSetup();
        public Biome1Setup Biome1 { get; set; } = new Biome1Setup();
        public OceanmonumentSetup Oceanmonument { get; set; } = new OceanmonumentSetup();
        public DungeonSetup Dungeon { get; set; } = new DungeonSetup();
        public DecorationSetup Decoration { get; set; } = new DecorationSetup();
        public LakeSetup Lake { get; set; } = new LakeSetup();
        public LavaLakeSetup LavaLake { get; set; } = new LavaLakeSetup();

        public string GetString()
        {
            var parts = new List<string>();
            parts.Add(string.Join(",", Layers.Select(t => t.IdString)));

            if (BiomeEnabled && Biome != null)
            {
                parts.Add(Biome.Id);
            }

            var features = new List<string>();
            if (Village.Enabled)
                features.Add(Village.SetupString);
            if (Stronghold.Enabled)
                features.Add(Stronghold.SetupString);
            if (Mineshaft.Enabled)
                features.Add(Mineshaft.SetupString);
            if (Biome1.Enabled)
                features.Add(Biome1.SetupString);
            if (Oceanmonument.Enabled)
                features.Add(Oceanmonument.SetupString);
            if (Dungeon.Enabled)
                features.Add(Dungeon.SetupString);
            if (Decoration.Enabled)
                features.Add(Decoration.SetupString);
            if (Lake.Enabled)
                features.Add(Lake.SetupString);
            if (LavaLake.Enabled)
                features.Add(LavaLake.SetupString);

            parts.Add(string.Join(",", features));
            parts.RemoveAll(string.IsNullOrEmpty);
            return string.Join(";", parts);
        }

        private static string Join(char separator = ' ', params (string, string)[] values)
        {
            return string.Join(separator.ToString(), values.Where(t => t.Item2 != null).Select(t => $"{t.Item1}={t.Item2}"));
        }
    }
}
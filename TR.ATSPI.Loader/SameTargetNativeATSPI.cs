using System;
using System.IO;
using System.Reflection;

using TR.ATSPI.Types;

namespace TR;

/// <summary>TargetPlatformが同じATSプラグインを操作します.</summary>
public class SameTargetNativeATSPI
{
	readonly DllManager DM;

	delegate void d_intArg(int arg);
	delegate Hand d_Elapse(State s, IntPtr Pa, IntPtr So);
	delegate uint d_GetPluginVersion();
	delegate void d_SetBeaconData(Beacon b);
	delegate void d_SetVehicleSpec(Spec s);

	readonly Action? PI_Dispose;
	readonly Action? PI_DoorClose;
	readonly Action? PI_DoorOpen;
	readonly d_Elapse? PI_Elapse;
	readonly d_GetPluginVersion? PI_GetPluginVersion;
	readonly d_intArg? PI_HornBlow;
	readonly d_intArg? PI_Initialize;
	readonly d_intArg? PI_KeyDown;
	readonly d_intArg? PI_KeyUp;
	readonly Action? PI_Load;
	readonly d_SetBeaconData? PI_SetBeaconData;
	readonly d_intArg? PI_SetBrake;
	readonly d_intArg? PI_SetPower;
	readonly d_intArg? PI_SetReverser;
	readonly d_intArg? PI_SetSignal;
	readonly d_SetVehicleSpec? PI_SetVehicleSpec;


	/// <summary>SameTargetATSPIインスタンスを初期化する</summary>
	/// <param name="PIPath">PIへのパス(絶対 or 相対)</param>
	public SameTargetNativeATSPI(in string PIPath)
	{
		//Ref : https://dobon.net/vb/dotnet/file/pathclass.html
		//ref : https://dobon.net/vb/dotnet/file/isabsolutepath.html

		DM = new DllManager(
			Path.IsPathRooted(PIPath) ?//絶対パスかどうか
				PIPath ://絶対パスならそのまま使用
				Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, PIPath)//相対パスなら, 絶対パスに変換して使用
			);

		PI_Dispose = DM.GetProcDelegate<Action>(nameof(Dispose));
		PI_DoorClose = DM.GetProcDelegate<Action>(nameof(DoorClose));
		PI_DoorOpen = DM.GetProcDelegate<Action>(nameof(DoorOpen));
		PI_Elapse = DM.GetProcDelegate<d_Elapse>(nameof(Elapse));
		PI_GetPluginVersion = DM.GetProcDelegate<d_GetPluginVersion>(nameof(GetPluginVersion));
		PI_HornBlow = DM.GetProcDelegate<d_intArg>(nameof(HornBlow));
		PI_Initialize = DM.GetProcDelegate<d_intArg>(nameof(Initialize));
		PI_KeyDown = DM.GetProcDelegate<d_intArg>(nameof(KeyDown));
		PI_KeyUp = DM.GetProcDelegate<d_intArg>(nameof(KeyUp));
		PI_Load = DM.GetProcDelegate<Action>(nameof(Load));
		PI_SetBeaconData = DM.GetProcDelegate<d_SetBeaconData>(nameof(SetBeaconData));
		PI_SetBrake = DM.GetProcDelegate<d_intArg>(nameof(SetBrake));
		PI_SetPower = DM.GetProcDelegate<d_intArg>(nameof(SetPower));
		PI_SetReverser = DM.GetProcDelegate<d_intArg>(nameof(SetReverser));
		PI_SetSignal = DM.GetProcDelegate<d_intArg>(nameof(SetSignal));
		PI_SetVehicleSpec = DM.GetProcDelegate<d_SetVehicleSpec>(nameof(SetVehicleSpec));
	}

	~SameTargetNativeATSPI() => DM.Dispose();//DllManagerは確実に解放する

	public void Dispose()
	{
		PI_Dispose?.Invoke();

		DM.Dispose();
	}

	public T? GetProcDelegate<T>(in string method) where T : class
		=> DM.GetProcDelegate<T>(method);

	public void DoorClose() => PI_DoorClose?.Invoke();
	public void DoorOpen() => PI_DoorOpen?.Invoke();
	public Hand Elapse(State s, IntPtr Pa, IntPtr So) => PI_Elapse?.Invoke(s, Pa, So) ?? default;
	public uint GetPluginVersion() => PI_GetPluginVersion?.Invoke() ?? ConstantValue.VersionNum;
	public void HornBlow(int k) => PI_HornBlow?.Invoke(k);
	public void Initialize(int s) => PI_Initialize?.Invoke(s);
	public void KeyDown(int k) => PI_KeyDown?.Invoke(k);
	public void KeyUp(int k) => PI_KeyUp?.Invoke(k);
	public void Load() => PI_Load?.Invoke();
	public void SetBeaconData(Beacon b) => PI_SetBeaconData?.Invoke(b);
	public void SetBrake(int b) => PI_SetBrake?.Invoke(b);
	public void SetPower(int p) => PI_SetPower?.Invoke(p);
	public void SetReverser(int r) => PI_SetReverser?.Invoke(r);
	public void SetSignal(int s) => PI_SetSignal?.Invoke(s);
	public void SetVehicleSpec(Spec s) => PI_SetVehicleSpec?.Invoke(s);
}

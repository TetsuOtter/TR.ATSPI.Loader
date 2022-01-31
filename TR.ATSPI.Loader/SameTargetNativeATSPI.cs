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

	/// <summary>アンマネージドなリソースを確実に解放する</summary>
	~SameTargetNativeATSPI() => DM.Dispose();//DllManagerは確実に解放する

	/// <summary>インスタンスが保持するリソースを解放する</summary>
	public void Dispose()
	{
		PI_Dispose?.Invoke();

		DM.Dispose();
	}

	/// <summary>DLLに含まれる関数を呼び出す</summary>
	/// <typeparam name="T">関数の型 (Generic型が含まれる場合は使用不可)</typeparam>
	/// <param name="method">関数名</param>
	/// <returns>関数のdelegate</returns>
	public T? GetProcDelegate<T>(in string method) where T : class
		=> DM.GetProcDelegate<T>(method);

	#region ATS-Plugin functions
	/// <summary>ドアが閉じた際に呼ぶ関数</summary>
	public void DoorClose() => PI_DoorClose?.Invoke();

	/// <summary>ドアが開いた際に呼ぶ関数</summary>
	public void DoorOpen() => PI_DoorOpen?.Invoke();

	/// <summary>毎フレーム実行される関数</summary>
	/// <param name="s">状態が保存された構造体</param>
	/// <param name="Pa">パネル配列</param>
	/// <param name="So">サウンド配列</param>
	/// <returns>ハンドル状態</returns>
	public Hand Elapse(State s, IntPtr Pa, IntPtr So) => PI_Elapse?.Invoke(s, Pa, So) ?? default;

	/// <summary>プラグインのバージョンを取得する</summary>
	/// <returns>プラグインのインターフェイスバージョン</returns>
	public uint GetPluginVersion() => PI_GetPluginVersion?.Invoke() ?? ConstantValue.VersionNum;

	/// <summary>警笛吹鳴時に呼び出す</summary>
	/// <param name="k">警笛の種類</param>
	public void HornBlow(int k) => PI_HornBlow?.Invoke(k);

	/// <summary>初期化時に呼び出す</summary>
	/// <param name="s">車両初期状態</param>
	public void Initialize(int s) => PI_Initialize?.Invoke(s);

	/// <summary>キー押下時に呼び出す</summary>
	/// <param name="k">キー番号</param>
	public void KeyDown(int k) => PI_KeyDown?.Invoke(k);

	/// <summary>キー解放時に呼び出す</summary>
	/// <param name="k">キー番号</param>
	public void KeyUp(int k) => PI_KeyUp?.Invoke(k);

	/// <summary>データ読み込み時に呼び出す</summary>
	public void Load() => PI_Load?.Invoke();

	/// <summary>地上子通過時に呼び出す</summary>
	/// <param name="b">地上子情報</param>
	public void SetBeaconData(Beacon b) => PI_SetBeaconData?.Invoke(b);

	/// <summary>ブレーキ操作時に呼び出す</summary>
	/// <param name="b">ブレーキ段数</param>
	public void SetBrake(int b) => PI_SetBrake?.Invoke(b);

	/// <summary>力行操作時に呼び出す</summary>
	/// <param name="p">ノッチ段数</param>
	public void SetPower(int p) => PI_SetPower?.Invoke(p);

	/// <summary>レバーサー操作時に呼び出す</summary>
	/// <param name="r">レバーサー段数</param>
	public void SetReverser(int r) => PI_SetReverser?.Invoke(r);

	/// <summary>信号現示が変化した際に呼び出す</summary>
	/// <param name="s">新しい信号現示</param>
	public void SetSignal(int s) => PI_SetSignal?.Invoke(s);

	/// <summary>車両のスペックを設定する</summary>
	/// <param name="s">スペック情報</param>
	public void SetVehicleSpec(Spec s) => PI_SetVehicleSpec?.Invoke(s);
	#endregion
}

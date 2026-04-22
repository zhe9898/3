using UnityEngine;
using UnityEngine.Events;

namespace CHGIS
{
    /// <summary>
    /// 时间轴控制器
    /// 管理年份切换，通知各渲染器更新
    /// </summary>
    public class TimeAxisController : MonoBehaviour
    {
        [Header("时间范围")]
        [Tooltip("秦统一")] public int minYear = -221;
        [Tooltip("清末")] public int maxYear = 1911;
        [Tooltip("默认年份")] public int defaultYear = 960;

        [Header("当前状态")]
        [SerializeField] private int currentYear;
        public int CurrentYear => currentYear;

        [Header("事件")]
        public UnityEvent<int> OnYearChanged;

        [Header("可选组件")]
        public BoundaryRenderer boundaryRenderer;
        public SettlementRenderer settlementRenderer;
        public RiverRenderer riverRenderer;

        void Start()
        {
            currentYear = defaultYear;
            RefreshAllRenderers();
        }

        /// <summary>
        /// 设置年份并刷新
        /// </summary>
        public void SetYear(int year)
        {
            year = Mathf.Clamp(year, minYear, maxYear);
            if (year == currentYear) return;

            currentYear = year;
            RefreshAllRenderers();
            OnYearChanged?.Invoke(currentYear);

            Debug.Log($"[TimeAxis] 年份切换至: {currentYear}");
        }

        /// <summary>
        /// 逐年递增
        /// </summary>
        public void StepForward(int steps = 1)
        {
            SetYear(currentYear + steps);
        }

        /// <summary>
        /// 逐年递减
        /// </summary>
        public void StepBackward(int steps = 1)
        {
            SetYear(currentYear - steps);
        }

        /// <summary>
        /// 跳转到指定朝代
        /// </summary>
        public void JumpToDynasty(Dynasty dynasty)
        {
            SetYear(dynasty switch
            {
                Dynasty.Qin => -221,
                Dynasty.Han => -202,
                Dynasty.Tang => 618,
                Dynasty.NorthernSong => 960,
                Dynasty.SouthernSong => 1127,
                Dynasty.Yuan => 1271,
                Dynasty.Ming => 1368,
                Dynasty.Qing => 1644,
                _ => currentYear
            });
        }

        void RefreshAllRenderers()
        {
            boundaryRenderer?.RenderYear(currentYear);
            settlementRenderer?.RenderYear(currentYear);
            riverRenderer?.RenderYear(currentYear);
        }
    }

    public enum Dynasty
    {
        Qin, Han, Tang, NorthernSong, SouthernSong,
        Yuan, Ming, Qing
    }
}

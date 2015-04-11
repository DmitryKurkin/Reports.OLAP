namespace WindowsFormsApplication1
{
    using RadarSoft.Common;
    using RadarSoft.WinForms;
    using RadarSoft.WinForms.Desktop;
    using System;
    using System.Windows.Forms;

    public partial class FormOlap : Form
    {
        private TOLAPCube _olapCube;

        public FormOlap()
        {
            InitializeComponent();

            // TODO: determine which one is really required
            tolapAnalysis1.OnInitHierarchy += tolapAnalysis1_OnInitHierarchy;
            tolapAnalysis1.OnAfterPivot += tolapAnalysis1_OnAfterPivot;
        }

        void tolapAnalysis1_OnAfterPivot(object sender, TPivotEventArgs e)
        {
            foreach (var d in tolapAnalysis1.Dimensions)
            {
                foreach (var h in d.Hierarchies)
                {
                    if (h.Levels != null)
                    {
                        foreach (var lev in h.Levels)
                        {
                            lev.SortType = TMembersSortType.msNameAsc;
                        }
                    }

                    h.SortType = TMembersSortType.msNameAsc;
                }
            }
        }

        private void userControlRadarSoftCubeCreator1_CubeCreated(object sender, EventArgs e)
        {
            tolapAnalysis1.Cube = userControlRadarSoftCubeCreator1.Cube;

            foreach (var d in tolapAnalysis1.Dimensions)
            {
                foreach (var h in d.Hierarchies)
                {
                    if (h.Levels != null)
                    {
                        foreach (var lev in h.Levels)
                        {
                            lev.SortType = TMembersSortType.msNameAsc;
                        }
                    }

                    h.SortType = TMembersSortType.msNameAsc;
                }
            }

            tolapAnalysis1.Cube.Active = true;
        }

        private void tolapAnalysis1_OnInitHierarchy(object sender, TEventInitHierarchyArgs e)
        {
            e.Hierarchy.SortType = TMembersSortType.msNameAsc;
            e.Hierarchy.Sort();
        }
    }
}
﻿namespace WindowsFormsControlLibraryRadarSoftCubeCreator
{
    public interface IExternalFilterProvider
    {
        void Reset();

        string GetFilter(string tableName, string fieldName, string fieldAlias);
    }
}
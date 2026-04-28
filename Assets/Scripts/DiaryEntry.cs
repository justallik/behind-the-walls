using UnityEngine;

public class DiaryEntry
{
    public int id;              // ID записи (1-9)
    public string title;        // Название записи
    public string content;      // Содержимое записи
    public string date;         // Дата записи
    public bool isNew = true;   // Является ли новой (для маркера NEW)
}

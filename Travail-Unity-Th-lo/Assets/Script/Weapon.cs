using UnityEngine;

public enum WeaponType { Sword, Axe, Bow }

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public int damage;
    public float attackSpeed; // Vitesse d'attaque (par ex. délai entre les coups)

    public GameObject attackEffectPrefab; // Préfabriqué pour l'attaque (flèche, impact de hache...)

    public GameObject weaponVisualPrefab;
}

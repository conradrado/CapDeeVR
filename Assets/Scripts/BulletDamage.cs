using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    /// 데미지와 총알의 생명 주기
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 3f;

    
    /// 충돌 시 실행되는 함수
    private void OnCollisionEnter(Collision collision)
{
    // 'collision'은 이 총알(bullet)이 부딪힌 상대방과 관련된 충돌 정보이다.
    // 'collision.gameObject'는 충돌한 상대 오브젝트를 가리킨다.

    // TryGetComponent는 상대 오브젝트(collision.gameObject)에 TargetEntity 컴포넌트가 붙어 있는지 확인한다.
    // 'out TargetEntity target'은 이 컴포넌트를 'target'이라는 변수에 출력(out)으로 저장하겠다는 뜻이다.
    // 즉, 상대방이 데미지를 받을 수 있는 타겟인지 확인하는 코드.
    if (collision.gameObject.TryGetComponent(out TargetEntity target))
    {
        // 만약 충돌한 상대가 TargetEntity 컴포넌트를 가지고 있다면,
        // 데미지를 입힌다.
        Debug.Log("데미지!");
        target.TakeDamage(damage);
    }

    // 총알은 한 번 충돌하면 사라진다.
    Destroy(gameObject, lifeTime);
}

}

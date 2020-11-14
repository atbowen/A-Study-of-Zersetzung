using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Email")]
public class EMail : ScriptableObject {
    public EMailAttachment[] attachments;

    public string senderInfo, receiverInfo;

    [TextArea]
    public string body, bodyEncrypted, bodyToShow;
    public Texture imageBody;
    public bool encrypted, isNew;

    [Range(0.0f, 100.0f)]
    public float risk;

    public char[] dangerKeys;
    public int hacksAttemptsAllowed;
    public List<char> keys;
    public List<char> values;

    public Dictionary<char, char> code = new Dictionary<char, char>();

    public int hackHealth;

    // Start is called before the first frame update
    void OnEnable()
    {
        bodyToShow = body;
        isNew = true;

        if ((keys.Count > 0) && (values.Count > 0) && keys.Count == values.Count) {
            for (int i = 0; i < keys.Count; i++) {
                code.Add(keys[i], values[i]);
            }
        }

        if (encrypted) {
            CreateEncryptedMessageWithCode(this);
        }

        hackHealth = hacksAttemptsAllowed;
    }

    public void CreateEncryptedMessageWithCode(EMail email) {

        char[] letters = body.ToCharArray();
        bodyEncrypted = "";

        if (code.Count > 0) {
            for (int i = 0; i < letters.Length; i++) {
                if (code.ContainsKey(letters[i])) {
                    letters[i] = code[letters[i]];
                }

                bodyEncrypted = bodyEncrypted + letters[i];
            }
        }
    }
}

using System.Collections;
using System.Threading.Tasks;

public class VideoManager : EventMonoBehaviour {
    protected override void StartOverride() {
        throw new System.NotImplementedException();
    }

    public Task ShowVideo() {
        return DoWaitFor(ShowVideoHelper);
    }

    protected IEnumerator ShowVideoHelper() {
        yield break;
    }
}

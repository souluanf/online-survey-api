import { initializeApp } from "https://www.gstatic.com/firebasejs/11.6.0/firebase-app.js";
import { getAuth, signInWithPopup, GoogleAuthProvider, signInWithEmailAndPassword, createUserWithEmailAndPassword, signOut, onAuthStateChanged } from "https://www.gstatic.com/firebasejs/11.6.0/firebase-auth.js";

const firebaseConfig = {
  apiKey: "AIzaSyDuLM1C0tWaPmQHRvwMHpY6Oj1ofrop_48",
  authDomain: "online-survey-8d0af.firebaseapp.com",
  projectId: "online-survey-8d0af",
  storageBucket: "online-survey-8d0af.firebasestorage.app",
  messagingSenderId: "978939585831",
  appId: "1:978939585831:web:14215abd8a35afe8bd3502"
};

const app = initializeApp(firebaseConfig);
const auth = getAuth(app);
let dotnetRef = null;

window.firebaseAuth = {
  initialize: (ref) => {
    dotnetRef = ref;
    onAuthStateChanged(auth, async (user) => {
      if (user) {
        const token = await user.getIdToken();
        await dotnetRef.invokeMethodAsync("OnAuthStateChanged", {
          uid: user.uid,
          email: user.email,
          displayName: user.displayName,
          token: token
        });
      } else {
        await dotnetRef.invokeMethodAsync("OnAuthStateChanged", null);
      }
    });
  },
  signInWithGoogle: async () => {
    const provider = new GoogleAuthProvider();
    const result = await signInWithPopup(auth, provider);
    const token = await result.user.getIdToken();
    return { uid: result.user.uid, email: result.user.email, displayName: result.user.displayName, token };
  },
  signInWithEmail: async (email, password) => {
    const result = await signInWithEmailAndPassword(auth, email, password);
    const token = await result.user.getIdToken();
    return { uid: result.user.uid, email: result.user.email, displayName: result.user.displayName, token };
  },
  registerWithEmail: async (email, password) => {
    const result = await createUserWithEmailAndPassword(auth, email, password);
    const token = await result.user.getIdToken();
    return { uid: result.user.uid, email: result.user.email, displayName: result.user.displayName, token };
  },
  signOut: async () => {
    await signOut(auth);
  },
  getToken: async () => {
    const user = auth.currentUser;
    if (!user) return null;
    return await user.getIdToken(true);
  }
};
